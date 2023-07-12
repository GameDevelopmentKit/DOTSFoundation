namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Interfaces;
    using Unity.Entities;

    public class AssetPathAuthoring : IAbilityActionComponentConverter
    {
        public string PrefabAssetName;
        public bool   IsUsingAddressable = true;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            if (this.IsUsingAddressable)
            {
                ecb.AddComponent(index, entity, new AddressablePathComponent() { Value = this.PrefabAssetName });
            }
            else
            {
                ecb.AddComponent(index, entity, new ViewPrefabEntityComponent() { Value = this.PrefabAssetName });
            }
        }
    }
}