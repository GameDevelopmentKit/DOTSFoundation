namespace GASCore.Services
{
    using Newtonsoft.Json;

    public static class EntitySerializerSetting
    {
        public static readonly JsonSerializerSettings Value = new JsonSerializerSettings()
        {
            TypeNameHandling    = TypeNameHandling.Auto,
            SerializationBinder = new KnownTypeBinder()
        };
    }
}