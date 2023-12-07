namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Baker;
    using GASCore.Interfaces;
    using Unity.Entities;

    public class AssetPathAuthoring : IAbilityActionComponentConverter
    {
        public string PrefabAssetName;
        public bool   IsUsingAddressable = true;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddViewAssetPathComponent(index, entity, this.PrefabAssetName, this.IsUsingAddressable);
        }
    }
}