namespace Transactions.Model
{
    using System.Collections.Generic;

    public struct TransactionResult
    {
        public List<Asset> Assets;
    }

    public struct Asset
    {
        public string AssetId;
        public int    Amount;
        public string AssetType;
    }
}