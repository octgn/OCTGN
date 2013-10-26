namespace Octgn.Launchers
{
    using System.Windows;

    using Octgn.DeckBuilder;

    public class DeckEditorLauncher : UpdatingLauncher
    {
        public override void BeforeUpdate()
        {
            
        }

        public override void AfterUpdate()
        {
            var win = new DeckBuilderWindow(null,true);
            Application.Current.MainWindow = win;
            win.Show();
        }
    }
}