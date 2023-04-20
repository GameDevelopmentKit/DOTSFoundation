namespace GASCore.Systems.SoundSystems.Components
{
    using DOTSCore.Extension;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public struct PlaySoundComponent : IComponentData
    {
        public FixedString64Bytes SoundAssetPath;
        public bool   IsLoop;
        public class _: IAbilityActionComponentConverter
        {
            public string SoundAssetPath;
            public bool   IsLoop;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index,entity,new PlaySoundComponent(){SoundAssetPath = this.SoundAssetPath,IsLoop = this.IsLoop});
            }
        }
    }
}