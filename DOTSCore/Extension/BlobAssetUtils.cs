namespace DOTSCore.Extension
{
    using Unity.Collections;
    using Unity.Entities;

    public static class BlobAssetUtils
    {
        public static BlobAssetReference<T> CreateReference<T>(this T value, Allocator allocator = Allocator.Persistent) where T : unmanaged {
            var builder = new BlobBuilder(Allocator.TempJob);
            ref var       data    = ref builder.ConstructRoot<T>();
            data = value;
            var reference = builder.CreateBlobAssetReference<T>(allocator);
            builder.Dispose();
 
            return reference;
        }
    }
}