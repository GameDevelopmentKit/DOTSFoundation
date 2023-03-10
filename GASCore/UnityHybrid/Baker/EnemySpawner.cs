namespace GASCore.UnityHybrid.Baker
{
    using System;
    using System.Collections.Generic;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public enum SpawnPosition
    {
        AroundFixedPosition,
        AroundCharacter,
    }

    [Serializable]
    public struct SpawnData : IBufferElementData
    {
        public FixedString64Bytes EnemyId;
        public int                Level;
        public int                Count;
        public float              Range;
        public FixedString64Bytes Key => EnemyId.Value + Level;
    }

    public class EnemySpawner : MonoBehaviour
    {
        public SpawnPosition Position = SpawnPosition.AroundCharacter;

        [ShowIf("Position", SpawnPosition.AroundFixedPosition)]
        public Transform FixedPosition;

        public List<SpawnData> Enemies;

        public class Baker : Baker<EnemySpawner>
        {
            public override void Bake(EnemySpawner authoring)
            {
                if (authoring.Position == SpawnPosition.AroundFixedPosition)
                {
                    this.AddComponent((Anchor)(float3)authoring.FixedPosition.position);
                }

                var spawnData = this.AddBuffer<SpawnData>();
                foreach (var data in authoring.Enemies)
                {
                    spawnData.Add(data);
                }
            }
        }
    }
}