namespace Octgn.Sdk.Extensibility.Desktop
{
    public interface IDesktopIntegration : IPlugin
    {
        MenuPlugin MainMenu(string gameId);
    }
}
