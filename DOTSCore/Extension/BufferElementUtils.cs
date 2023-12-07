namespace DOTSCore.Extension
{
    using Unity.Entities;

    [InternalBufferCapacity(10)]
    public readonly struct BufferElement<T> : IBufferElementData where T : struct {
        public readonly T Value;
 
        public BufferElement(T value) {
            this.Value = value;
        }
    }
}