namespace Transactions.Manager
{
    using GameFoundation.Scripts.Utilities.Extension;
    using Transactions.PaymentService;
    using Transactions.PayoutService;
    using Zenject;

    public class TransactionInstaller : Installer<TransactionInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<ITransactionManager>().To<TransactionManager>().AsSingle();
            this.Container.BindInterfacesAndSelfToAllTypeDriveFrom<IPaymentService>();
            this.Container.BindInterfacesAndSelfToAllTypeDriveFrom<IPayoutService>();
        }
    }
}