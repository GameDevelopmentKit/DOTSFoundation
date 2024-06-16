namespace DOTSCore.CommonSystems.Baker
{
    using System.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    public class EntityBehaviour : MonoBehaviour
    {
        [SerializeField] protected bool   syncEntityTransform;
        protected                  Entity LinkedEntity;

        void Awake()
        {
            this.StartCoroutine(this.Initialize());
            this.AwakeInternal();
        }
        
        private IEnumerator Initialize()
        {
            var world   = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                yield return new WaitUntil(() => World.DefaultGameObjectInjectionWorld != null);
                world = World.DefaultGameObjectInjectionWorld;
            }
            
            var manager = world.EntityManager;
            
            this.LinkedEntity = manager.CreateEntity();
            manager.SetName(this.LinkedEntity, this.name + "Entity");

            if (this.syncEntityTransform)
            {
                manager.AddComponentData(this.LinkedEntity, new LocalTransform
                {
                    Position = this.transform.position,
                    Rotation = this.transform.rotation,
                    Scale    = 1,
                });

                // Transform access requires this
                manager.AddComponentObject(this.LinkedEntity, this.transform);
            }

            this.BindEntity(manager, this.LinkedEntity);
        }

        void OnDestroy()
        {
            if (!this.ValidateWorld(out var world)) return;

            var manager = world.EntityManager;
            manager.DestroyEntity(this.LinkedEntity);
            this.OnDestroyInternal();
        }

        void OnEnable()
        {
            if (!this.ValidateWorld(out var world)) return;

            var manager = world.EntityManager;
            manager.SetEnabled(this.LinkedEntity, true);
            this.OnEnableInternal();
        }

        void OnDisable()
        {
            if (!this.ValidateWorld(out var world)) return;

            var manager = world.EntityManager;
            manager.SetEnabled(this.LinkedEntity, false);
            this.OnDisableInternal();
        }
        private bool ValidateWorld(out World world)
        {
            world = World.DefaultGameObjectInjectionWorld;
            return world != null && this.LinkedEntity != Entity.Null;
        }

        protected virtual void AwakeInternal() { }
        
        protected virtual void OnEnableInternal() { }
        
        protected virtual void OnDisableInternal() { }
        
        protected virtual void OnDestroyInternal() { }

        protected virtual void BindEntity(EntityManager entityManager, Entity entity) { }
    }
}