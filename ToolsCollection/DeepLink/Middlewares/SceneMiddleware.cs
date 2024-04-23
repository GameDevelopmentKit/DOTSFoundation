
namespace DeepLink.Middlewares
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;

    public class SceneMiddleware : IDeepLinkMiddleware
    {
        private readonly SceneDirector sceneDirector;
        public           RouteType     Type => RouteType.Scene;

        public SceneMiddleware(SceneDirector sceneDirector)
        {
            this.sceneDirector = sceneDirector;
        }
        public void Process(string sceneName)
        {
            if (SceneDirector.CurrentSceneName != sceneName) this.sceneDirector.LoadSingleSceneBySceneManagerAsync(sceneName).Forget();
        }
    }
}