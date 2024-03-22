namespace Transactions.PayoutService
{
    using Cysharp.Threading.Tasks;
    using Transactions.Model;

    public interface IPayoutService
    {
        string  AssetType { get; }
        UniTask ReceivePayout(Asset asset);
    }
}