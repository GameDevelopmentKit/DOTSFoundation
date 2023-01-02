namespace DOTSCore.EntityFactory
{
    using System;
    using System.Runtime.InteropServices;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public abstract class BaseEntityFactoryByEcb<TData> : IEntityFactoryByEcb<TData>
    {
        protected World CurrentWorld;
        public BaseEntityFactoryByEcb() { this.CurrentWorld = World.DefaultGameObjectInjectionWorld; }

        public virtual Entity CreateEntity(EntityCommandBuffer.ParallelWriter ecb, int index, TData data, params ComponentType[] types)
        {
            var entity = ecb.CreateEntity(index, this.CurrentWorld.EntityManager.CreateArchetype(types));
            this.InitComponents(ref ecb, index, ref entity, data);
            return entity;
        }

        protected abstract void InitComponents(ref EntityCommandBuffer.ParallelWriter ecb, in int index, ref Entity entity, in TData data);

        public EntityFactoryResolver<TData> Resolver => new(new FunctionPointer<InitDelegate>(Marshal.GetFunctionPointerForDelegate<InitDelegate>(this.InitComponents)),
            new NativeArray<int>(1, Allocator.Persistent) { [0] = 0 });

        public delegate void InitDelegate(ref EntityCommandBuffer.ParallelWriter ecb, in int index, ref Entity entity, in TData data);
    }

    [BurstCompile]
    public struct EntityFactoryResolver<TData> : IDisposable
    {
        private          NativeArray<int>                                            placeholder;
        private readonly FunctionPointer<BaseEntityFactoryByEcb<TData>.InitDelegate> initFunction;

        public EntityFactoryResolver(FunctionPointer<BaseEntityFactoryByEcb<TData>.InitDelegate> initFunction, NativeArray<int> placeholder)
        {
            this.initFunction = initFunction;
            this.placeholder  = placeholder;
        }

        public Entity CreateEntity(ref EntityCommandBuffer.ParallelWriter ecb, in int index, in TData data, EntityArchetype entityArchetype = default)
        {
            var result = entityArchetype.Equals(default) ? ecb.CreateEntity(index) : ecb.CreateEntity(index, entityArchetype);
            this.initFunction.Invoke(ref ecb, index, ref result, data);
            return result;
        }
        public void Dispose() { this.placeholder.Dispose(); }
    }

    public abstract class BaseEntityPrefabFactoryByEcb<TData> : BaseEntityFactoryByEcb<TData>
    {
        public override Entity CreateEntity(EntityCommandBuffer.ParallelWriter ecb, int index, TData data, params ComponentType[] types)
        {
            var entity = ecb.CreateEntity(index, this.CurrentWorld.EntityManager.CreateArchetype(types));
            ecb.AddComponent(index, entity, typeof(Prefab));
            ecb.AddComponent(index, entity, new Rotation() { Value     = quaternion.identity });
            ecb.AddComponent(index, entity, new Translation() { Value  = float3.zero });
            ecb.AddComponent(index, entity, new LocalToWorld() { Value = float4x4.identity });
            this.InitComponents(ref ecb, index, ref entity, data);
            return entity;
        }
    }
}