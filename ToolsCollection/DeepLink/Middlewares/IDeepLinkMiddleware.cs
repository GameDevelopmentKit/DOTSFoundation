namespace DeepLink.Middlewares
{
    public interface IDeepLinkMiddleware
    {
        RouteType Type { get; }
        void Process(string info);
    }

    public enum RouteType
    {
        Scene,
        Building
    }
}