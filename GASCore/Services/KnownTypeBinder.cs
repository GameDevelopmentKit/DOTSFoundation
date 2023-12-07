namespace GASCore.Services
{
    using System;
    using Newtonsoft.Json.Serialization;

    public class KnownTypeBinder : ISerializationBinder
    {
        public Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType($"{typeName}, {assemblyName}");
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.Assembly.FullName;
            typeName     = serializedType.FullName;
        }
    }
}