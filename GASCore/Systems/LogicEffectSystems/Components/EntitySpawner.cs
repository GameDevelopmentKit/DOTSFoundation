namespace GASCore.Systems.LogicEffectSystems.Components
{
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using Unity.Entities;
    using Zenject;

    public struct EntitySpawner : IComponentData
    {
        public int    MaxAmount;
        public Entity EntityPrefab;
        public bool   IsDrop;
        public bool   IsRandomAmount;
    }

    public class EntitySpawnerAuthoring : IAbilityActionComponentConverter
    {
        [Inject] private AbilityActionEntityPrefabFactory actionEntityPrefabFactory;

        public int  MaxAmount;
        public bool IsDrop;
        public bool IsRandomAmount;

        public EntityConverter.EntityData<IComponentConverter> EntityPrefab;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ZenjectUtils.GetCurrentContainer()?.Inject(this);
            var entityPrefab = this.actionEntityPrefabFactory.CreateEntity(ecb, index, this.EntityPrefab.components);

            ecb.AddComponent(index, entity, new EntitySpawner()
            {
                MaxAmount      = this.MaxAmount,
                EntityPrefab   = entityPrefab,
                IsDrop         = this.IsDrop,
                IsRandomAmount = this.IsRandomAmount
            });
            ecb.SetParent(index, entityPrefab, entity);
        }
    }
}