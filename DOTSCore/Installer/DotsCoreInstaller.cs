namespace DOTSCore.Installer
{
    using DOTSCore.PrefabWorkflow;
    using DOTSCore.World;
    using Unity.Entities;
    using Zenject;

    public class DotsCoreInstaller : Installer<DotsCoreInstaller>
    {
        public override void InstallBindings()
        {
            // this.Container.BindInterfacesTo<GameWorldController>().AsCached().NonLazy();
            // this.Container.Bind<PrefabDatabase>().FromScriptableObjectResource(nameof(PrefabDatabase)).AsCached().NonLazy();
        }
    }
}