namespace DeepLink
{
    using DeepLink.Middlewares;
    using GameFoundation.Scripts.Utilities.Extension;
    using Zenject;
    public class DeepLinkInstaller : Installer<DeepLinkInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<DeepLinkProcessing>().AsSingle().NonLazy();

            this.Container.BindInterfacesAndSelfToAllTypeDriveFrom<IDeepLinkMiddleware>();

            this.Container.DeclareSignal<DeepLinkSignal>();
        }
    }
}