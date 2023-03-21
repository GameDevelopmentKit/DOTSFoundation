namespace GASCore.Systems.EntityGeneratorSystems.Components
{
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using Zenject;

    public struct EntitySpawner : IComponentData
    {
        public Entity EntityPrefab;
        public float  SpawnChance;

        public bool IsDrop;
        public bool IsSetChild;
        public bool IsLookSpawnerRotation;
        public bool IsResetRotationAfterSpawn;

        public float SpawnerRadius;

        public SimpleIntRange   AmountRange;
        public SimpleFloatRange StartAngleRange;
        public SimpleFloatRange AngleStepRange;

        public float CurrentAngle;
        public int   Clockwise;
    }

    public class EntitySpawnerAuthoring : IAbilityActionComponentConverter
    {
        [Inject] private AbilityActionEntityPrefabFactory actionEntityPrefabFactory;

        [Range(0f, 1f)] public float SpawnChance = 1f;

        public bool  IsDrop                    = false;
        public bool  IsSetChild                = false;
        public bool  IsLookSpawnerRotation     = false;
        public bool  IsResetRotationAfterSpawn = false;
        public float SpawnerRadius             = 0;
        
        public SimpleIntRange   AmountRange     = new() { min = 1, max = 1 };
        public SimpleFloatRange StartAngleRange = new() { min = 0, max = 0 };
        public SimpleFloatRange AngleStepRange  = new() { min = 0, max = 0 };

        public EntityConverter.EntityData<IComponentConverter> EntityPrefab;

        public bool AttachToAffectedTarget = true;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            this.GetCurrentContainer()?.Inject(this);

            var entityPrefab = this.actionEntityPrefabFactory.CreateEntity(ecb, index, this.EntityPrefab.components);
            ecb.SetParent(index, entityPrefab, entity);
            ecb.AddComponent(index, entity, new EntitySpawner()
            {
                EntityPrefab              = entityPrefab,
                SpawnChance               = this.SpawnChance,
                IsDrop                    = this.IsDrop,
                IsSetChild                = this.IsSetChild,
                IsLookSpawnerRotation     = this.IsLookSpawnerRotation,
                IsResetRotationAfterSpawn = this.IsResetRotationAfterSpawn,
                SpawnerRadius             = this.SpawnerRadius,
                
                AmountRange           = this.AmountRange,
                StartAngleRange       = new SimpleFloatRange() { min = math.radians(this.StartAngleRange.min), max = math.radians(this.StartAngleRange.max) },
                AngleStepRange        = new SimpleFloatRange() { min = math.radians(this.AngleStepRange.min), max  = math.radians(this.AngleStepRange.max) },

                Clockwise = 0,
            });
            if(this.AttachToAffectedTarget)
                ecb.AddComponent<AttachToAffectedTarget>(index, entity);
        }
    }
}