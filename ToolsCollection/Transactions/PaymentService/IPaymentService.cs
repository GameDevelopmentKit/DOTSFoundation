namespace Transactions.PaymentService
{
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;

    public interface IPaymentService
    {
        PaymentType PaymentType { get; }

        /// <summary>
        ///  Check payment service has this assetId available
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        bool Available(string assetId);
        
        
        /// <summary>
        /// Test whether the costs can be met by the currencies in the payment service.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool VerifyCost(string assetId, float value);

        /// <summary>
        /// Proceed to deduct Cost with the Payment Service
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="value"></param>
        /// <returns>Remain value</returns>
        UniTask<float> MakePayment(string assetId, float value);
    }
}