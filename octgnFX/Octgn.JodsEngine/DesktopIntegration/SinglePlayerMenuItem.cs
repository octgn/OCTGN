//using Octgn.Sdk.Extensibility;
//using Octgn.Sdk.Extensibility.Desktop;
//using System.Threading.Tasks;
//using System.Windows;

//namespace Octgn.DesktopIntegration
//{
//    public class SinglePlayerMenuItem : MenuItem
//    {
//        public override Task OnClick(ClickContext context) {
//            if(context.GamePlugin is ISinglePlayerGameMode singlePlayerStarter) {
//                return singlePlayerStarter.StartSinglePlayer();
//            } else {
//                MessageBox.Show($"Game {context.GamePlugin} does not have a single player mode", "404", MessageBoxButton.OK, MessageBoxImage.Warning);

//                return Task.CompletedTask;
//            }
//        }
//    }
//}
