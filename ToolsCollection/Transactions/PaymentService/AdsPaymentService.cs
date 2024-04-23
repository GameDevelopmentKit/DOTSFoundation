namespace Transactions.PaymentService
{
    using Cysharp.Threading.Tasks;
    using Transactions.Blueprint;

    public class AdsPaymentService : IPaymentService
    {
        public PaymentType PaymentType => PaymentType.Ads;
        public bool Available() { return false; }
        public bool VerifyCost(CostRecord costRecord) { return true; }
        public UniTask MakePayment(CostRecord costRecord) { return UniTask.CompletedTask; }
    }
}