namespace Wallet.Blueprint
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("Resource")]
    public class ResourceBlueprint : GenericBlueprintReaderByRow<string, ResourceRecord> { }

    [CsvHeaderKey("Id")]
    public class ResourceRecord
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Icon;
        public readonly string Description;
        public readonly int    DefaultValue;
    }
}