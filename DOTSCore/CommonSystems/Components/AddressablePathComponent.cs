namespace DOTSCore.CommonSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct AddressablePathComponent : IComponentData
    {
        public FixedString128Bytes Value;
    }
}