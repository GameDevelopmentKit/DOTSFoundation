namespace DeepLink
{
    using DeepLink.Blueprint;
    using DeepLink.Middlewares;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Zenject;

    public class DeepLinkProcessing : IInitializable
    {
        public const string URL_HOST = "unitydl://monster-hunter-takeover/";

        private readonly DeepLinkBlueprint blueprint;
        private readonly SignalBus signalBus;
        private readonly DiContainer diContainer;
        private Dictionary<RouteType, IDeepLinkMiddleware> middlewares;

        public DeepLinkProcessing(DiContainer diContainer, SignalBus signalBus, DeepLinkBlueprint blueprint)
        {
            this.signalBus = signalBus;
            this.diContainer = diContainer;
            this.blueprint = blueprint;

            Application.deepLinkActivated += OnDeepLinkActivated;
            this.signalBus.Subscribe<DeepLinkSignal>(this.OnDeepLinkActivated);

            if (!string.IsNullOrEmpty(Application.absoluteURL)) OnDeepLinkActivated(Application.absoluteURL);
        }

        public void Initialize()
        {
            middlewares = this.diContainer.ResolveAll<IDeepLinkMiddleware>().ToDictionary(middleware => middleware.Type);
        }

        private void OnDeepLinkActivated(DeepLinkSignal signal)
        {
            if(this.blueprint.TryGetValue(signal.DeepLinkId, out var record)) OnDeepLinkActivated(record.Url);
        }

        private void OnDeepLinkActivated(string url)
        {
            var mainInfo = url.Replace(URL_HOST, string.Empty);

            var splitMainInfo = mainInfo.Split("/");
            if (Enum.TryParse<RouteType>(splitMainInfo[0], true, out var route) && GetMiddleware(route, out var middleware))
            {
                middleware.Process(mainInfo.Replace($"{splitMainInfo[0]}/", string.Empty));
            }
        }

        private bool GetMiddleware(RouteType routeType, out IDeepLinkMiddleware middleware)
        {
            return this.middlewares.TryGetValue(routeType, out middleware);
        }
    }
}