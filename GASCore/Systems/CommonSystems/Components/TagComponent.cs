namespace GASCore.Systems.CommonSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct TagComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }
}