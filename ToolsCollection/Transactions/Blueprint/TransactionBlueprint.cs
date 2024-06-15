namespace Transactions.Blueprint
{
    using DataManager.Blueprint.BlueprintReader;

    [BlueprintReader("Transaction")]
    public class TransactionBlueprint : GenericBlueprintReaderByRow<string, TransactionRecord> { }

    [CsvHeaderKey("Id")]
    public struct TransactionRecord
    {
        public string Id;
        public BlueprintByRow<CostRecord> Costs;
        public BlueprintByRow<PayoutRecord> Payouts;
    }

    [CsvHeaderKey("PaymentType")]
    public struct CostRecord
    {
        public string CostAssetId;
        public float Amount;
        public PaymentType PaymentType;
    }

    [CsvHeaderKey("AssetType")]
    public struct PayoutRecord
    {
        public string PayoutAssetId;
        public int MinAmount;
        public int MaxAmount;
        public float Chance;
        public string AssetType;
    }

    public static class  AssetType
    {
        public const string Currency = "Currency";
        public const string Item = "Item";
    }

    public enum PaymentType
    {
        Currency,
        Item,
        IAP,
        Ads
    }
}