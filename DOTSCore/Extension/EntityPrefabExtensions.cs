namespace DOTSCore.Extension
{
    using DOTSCore.PrefabWorkflow;
    using Unity.Entities;

    public static class EntityPrefabExtensions
    {
        /// <summary>Gets a prefab with the specified singleton component type.</summary>
        public static Entity GetPrefab<T>(this EntityManager entityManager) where T : struct, IComponentData
            => entityManager.QueryPrefab<T>().GetSingletonEntity();

        public static EntityQuery QueryPrefab<T>(this EntityManager entityManager) where T : struct, IComponentData
            => entityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(T)),
                ComponentType.ReadOnly(typeof(Prefab))
            );
        
        public static bool HasPrefab<T>(this EntityManager entityManager) where T : struct, IComponentData
            => !entityManager.QueryPrefab<T>().IsEmpty;
        
        /// <summary>Gets the singleton data of the specified component type.</summary>
        public static T GetSingleton<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
            => entityManager.CreateEntityQuery(typeof(T)).GetSingleton<T>();
        
        public static Entity GetSingletonEntity<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
            => entityManager.CreateEntityQuery(typeof(T)).GetSingletonEntity();


        /// <summary>Gets a buffer of prefabs with a group that has the specified singleton component type.</summary>
        public static DynamicBuffer<PrefabPool> GetPrefabs<T>(this EntityManager entityManager) where T : struct, IComponentData
        {
            var group = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(T)),
                ComponentType.ReadOnly(typeof(PrefabPool))
            ).GetSingletonEntity();

            return entityManager.GetBuffer<PrefabPool>(group);
        }
    }
}