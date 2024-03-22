namespace Transactions.PaymentService
{
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;

    public interface IPaymentService
    {
        PaymentType PaymentType { get; }

        bool Available();
        
        /// <summary>
        ///   Test whether the costs can be met by the currencies in the payment service.
        /// </summary>
        /// <param name="costRecord"></param>
        /// <returns></returns>
        bool VerifyCost(CostRecord costRecord);

        /// <summary>
        ///  Proceed to deduct Cost with the Payment Service
        /// </summary>
        /// <param name="costRecord"></param>
        UniTask MakePayment(CostRecord costRecord);
    }
}