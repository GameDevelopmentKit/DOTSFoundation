namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine.Jobs;

    //copy from Unity.Entities.CompanionGameObjectUpdateTransformSystem but modify a bit
    struct SyncGameObjectTransformCleanup : ICleanupComponentData
    {
    }

    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(LateSimulationSystemGroup))]
    [BurstCompile]
    partial class SyncGameObjectTransformSystem : SystemBase
    {
        struct IndexAndInstance
        {
            public int transformAccessArrayIndex;
            public int instanceID;
        }

        TransformAccessArray                    m_TransformAccessArray;
        NativeList<Entity>                      m_Entities;
        NativeHashMap<Entity, IndexAndInstance> m_EntitiesMap;

        EntityQuery m_CreatedQuery;
        EntityQuery m_DestroyedQuery;

        protected override void OnCreate()
        {
            m_TransformAccessArray = new TransformAccessArray(0);
            m_Entities             = new NativeList<Entity>(64, Allocator.Persistent);
            m_EntitiesMap          = new NativeHashMap<Entity, IndexAndInstance>(64, Allocator.Persistent);
            m_CreatedQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    All  = new[] { ComponentType.ReadOnly<GameObjectHybridLink>() },
                    None = new[] { ComponentType.ReadOnly<SyncGameObjectTransformCleanup>() }
                }
            );
            m_DestroyedQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    All  = new[] { ComponentType.ReadOnly<SyncGameObjectTransformCleanup>() },
                    None = new[] { ComponentType.ReadOnly<AssetPathComponent>() }
                }
            );
        }

        protected override void OnDestroy()
        {
            m_TransformAccessArray.Dispose();
            m_Entities.Dispose();
            m_EntitiesMap.Dispose();
        }

        struct RemoveDestroyedEntitiesArgs
        {
            public EntityQuery                             DestroyedQuery;
            public NativeList<Entity>                      Entities;
            public NativeHashMap<Entity, IndexAndInstance> EntitiesMap;
            public TransformAccessArray                    TransformAccessArray;
            public EntityManager                           EntityManager;
        }

        [BurstCompile]
        static void RemoveDestroyedEntities(ref RemoveDestroyedEntitiesArgs args)
        {
            var entities = args.DestroyedQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                // This check is necessary because the code for adding entities is conditional and in edge-cases where
                // objects are quickly created-and-destroyed, we might not have the entity in the map.
                if (args.EntitiesMap.TryGetValue(entity, out var indexAndInstance))
                {
                    var index = indexAndInstance.transformAccessArrayIndex;
                    args.TransformAccessArray.RemoveAtSwapBack(index);
                    args.Entities.RemoveAtSwapBack(index);
                    args.EntitiesMap.Remove(entity);
                    if (index < args.Entities.Length)
                    {
                        var fixup = args.EntitiesMap[args.Entities[index]];
                        fixup.transformAccessArrayIndex        = index;
                        args.EntitiesMap[args.Entities[index]] = fixup;
                    }
                }
            }

            entities.Dispose();
            args.EntityManager.RemoveComponent<SyncGameObjectTransformCleanup>(args.DestroyedQuery);
        }

        protected override void OnUpdate()
        {
            if (!m_CreatedQuery.IsEmpty)
            {
                var entities = m_CreatedQuery.ToEntityArray(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var link   = EntityManager.GetComponentData<GameObjectHybridLink>(entity);

                    // It is possible that an object is created and immediately destroyed, and then this shouldn't run.
                    if (link.Value != null && !EntityManager.HasComponent<IgnoreSysnTransformComponent>(entity))
                    {
                        IndexAndInstance indexAndInstance = default;
                        indexAndInstance.transformAccessArrayIndex = m_Entities.Length;
                        indexAndInstance.instanceID                = link.Value.GetInstanceID();
                        m_EntitiesMap.Add(entity, indexAndInstance);
                        m_TransformAccessArray.Add(link.Value.transform);
                        m_Entities.Add(entity);
                    }
                }

                entities.Dispose();
                EntityManager.AddComponent<SyncGameObjectTransformCleanup>(m_CreatedQuery);
            }

            if (!m_DestroyedQuery.IsEmpty)
            {
                var args = new RemoveDestroyedEntitiesArgs
                {
                    Entities             = m_Entities,
                    DestroyedQuery       = m_DestroyedQuery,
                    EntitiesMap          = m_EntitiesMap,
                    EntityManager        = EntityManager,
                    TransformAccessArray = m_TransformAccessArray
                };
                RemoveDestroyedEntities(ref args);
            }

            Dependency = new CopyTransformJob
            {
                localToWorld    = GetComponentLookup<LocalToWorld>(),
                entities        = m_Entities,
                ignoreTransform = GetComponentLookup<IgnoreSysnTransformComponent>()
            }.Schedule(m_TransformAccessArray, Dependency);
        }

        [BurstCompile]
        struct CopyTransformJob : IJobParallelForTransform
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalToWorld>                 localToWorld;
            [ReadOnly]                            public NativeList<Entity>                            entities;
            [NativeDisableParallelForRestriction] public ComponentLookup<IgnoreSysnTransformComponent> ignoreTransform;
            public unsafe void Execute(int index, TransformAccess transform)
            {
                var entity = this.entities[index];
                if (!this.ignoreTransform.HasComponent(entity))
                {
                    var ltw = localToWorld[entity];
                    var mat = *(UnityEngine.Matrix4x4*)&ltw;
                    transform.localPosition = ltw.Position;
                    transform.localRotation = mat.rotation;
                    transform.localScale    = mat.lossyScale;
                }
            }
        }
    }
}