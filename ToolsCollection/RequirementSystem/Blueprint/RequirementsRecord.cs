namespace RequirementSystem.Blueprint
{
    using BlueprintFlow.BlueprintReader;

    [CsvHeaderKey("RequirementId")]
    public struct RequirementsRecord
    {
        public readonly string RequirementId;
        public readonly int    RequirementValue;
        public readonly string RequirementType;
    }
}