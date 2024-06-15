namespace RequirementSystem.Blueprint
{
    using DataManager.Blueprint.BlueprintReader;

    [CsvHeaderKey("RequirementId")]
    public struct RequirementsRecord
    {
        public readonly string RequirementId;
        public readonly int    RequirementValue;
        public readonly string RequirementType;
    }
}