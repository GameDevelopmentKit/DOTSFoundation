namespace DOTSCore.CommonSystems.Baker
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class ViewAssetPath : MonoBehaviour
    {
        public string AddressablePath;
    }

    public class ViewAssetPathBaker : Baker<ViewAssetPath>
    {
        public override void Bake(ViewAssetPath authoring) { this.AddComponent(this.GetEntity(TransformUsageFlags.Dynamic),new AddressablePathComponent() { Value = authoring.AddressablePath }); }
    }
}