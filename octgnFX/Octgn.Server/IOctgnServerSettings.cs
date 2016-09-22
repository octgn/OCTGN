namespace Octgn.Server
{
    public interface IOctgnServerSettings
    {
        bool IsLocalGame { get; }
        string ApiKey { get; }
    }
}