namespace GASCore.Systems.SoundSystems.Components
{
    using GASCore.Interfaces;
    using global::System;
    using Unity.Entities;

    public struct PlaySoundComponent : ISharedComponentData, IEquatable<PlaySoundComponent>
    {
        public string SoundAssetPath;
        public bool   IsLoop;
        public class _: IAbilityActionComponentConverter
        {
            public string SoundAssetPath;
            public bool   IsLoop;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddSharedComponentManaged(index,entity,new PlaySoundComponent(){SoundAssetPath = this.SoundAssetPath,IsLoop = this.IsLoop});
            }
        }

        public bool Equals(PlaySoundComponent other)
        {
            return this.SoundAssetPath == other.SoundAssetPath && this.IsLoop == other.IsLoop;
        }
        public override bool Equals(object obj)
        {
            return obj is PlaySoundComponent other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(this.SoundAssetPath, this.IsLoop);
        }
    }
}