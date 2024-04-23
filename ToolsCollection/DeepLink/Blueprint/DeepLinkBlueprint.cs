namespace DeepLink.Blueprint
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("DeepLink")]
    public class DeepLinkBlueprint : GenericBlueprintReaderByRow<string, DeepLinkRecord> { }

    [CsvHeaderKey("DeepLinkId")]
    public class DeepLinkRecord
    {
        public readonly string DeepLinkId;
        public readonly string Url;
    }
}