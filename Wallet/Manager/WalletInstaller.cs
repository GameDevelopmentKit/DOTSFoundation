namespace Wallet.Manager
{
    using Wallet.Services;
    using Zenject;

    public class WalletInstaller : Installer<WalletInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<NotifyCurrencyService>().AsSingle().NonLazy();
            this.Container.Bind<IWalletManager>().To<WalletManager>().AsSingle();
        }
    }
}