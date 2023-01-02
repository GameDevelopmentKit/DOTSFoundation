namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Interfaces;
    using Unity.Entities;

    public class AssetPathAuthoring : IAbilityActionComponentConverter
    {
        public string PrefabAssetName;
        public void   Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new AssetPathComponent() { Path = this.PrefabAssetName }); }
    }
}