namespace Transactions.Blueprint
{
    using DataManager.Blueprint.BlueprintReader;
    using UnityEngine;

    [CsvHeaderKey("TransactionId")]
    public struct TransactionRecord
    {
        public string                       TransactionId;
        public BlueprintByRow<CostRecord>   Costs;
        public BlueprintByRow<PayoutRecord> Payouts;
    }

    [CsvHeaderKey("PaymentType")]
    public struct CostRecord
    {
        public string      CostAssetId;
        public float       CostAmount;
        public PaymentType PaymentType;
    }

    public abstract class PayoutRecordAbstract
    {
        public string PayoutAssetId { get; set; }
        public float  Chance        { get; set; }
        public string AssetType     { get; set; }

        public abstract int GetAmount();
    }

    [CsvHeaderKey("AssetType")]
    public class PayoutRecord : PayoutRecordAbstract
    {
        public          int PayoutAmount { get; set; }
        public override int GetAmount()  => PayoutAmount;
    }

    [CsvHeaderKey("AssetType")]
    public class PayoutMinMaxRecord : PayoutRecordAbstract
    {
        public          int MinAmount   { get; set; }
        public          int MaxAmount   { get; set; }
        public override int GetAmount() => Random.Range(MinAmount, MaxAmount);
    }

    public static class AssetDefaultType
    {
        public const string Currency = "Currency";
        public const string Item     = "Item";
    }

    public enum PaymentType
    {
        Currency,
        Item,
        IAP,
        Ads
    }
}