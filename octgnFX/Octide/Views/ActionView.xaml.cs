using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Octide.ViewModel;
using System.Windows.Input;
using System.Text;

namespace Octide.Views
{
    /// <summary>
    /// Interaction logic for CounterTabView.xaml
    /// </summary>
    public partial class ActionView : UserControl
    {
        public ActionView()
        {
            InitializeComponent();
        }

        private void ShortCutBoxKeyDown(object sender, KeyEventArgs e)
        {
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Ignore modifier keys.
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            // Build the shortcut key name.
            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key.ToString());

            // Update the text box.
            ShortcutBox.Text = shortcutText.ToString();
        }
    }
}