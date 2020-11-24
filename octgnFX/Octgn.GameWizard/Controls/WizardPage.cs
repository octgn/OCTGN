using Octgn.GameWizard.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.GameWizard.Controls
{
    public class WizardPage : UserControl
    {
        public bool ForwardEnabled {
            get { return (bool)GetValue(ForwardEnabledProperty); }
            set { SetValue(ForwardEnabledProperty, value); }
        }

        public static readonly DependencyProperty ForwardEnabledProperty =
            DependencyProperty.Register(nameof(ForwardEnabled), typeof(bool), typeof(WizardPage), new PropertyMetadata(true));

        public bool BackEnabled {
            get { return (bool)GetValue(BackEnabledProperty); }
            set { SetValue(BackEnabledProperty, value); }
        }

        public static readonly DependencyProperty BackEnabledProperty =
            DependencyProperty.Register(nameof(BackEnabled), typeof(bool), typeof(WizardPage), new PropertyMetadata(true));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(WizardPage), new PropertyMetadata(nameof(WizardPage)));

        public NewGame Game {
            get { return (NewGame)GetValue(GameProperty); }
            set { SetValue(GameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Game.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("Game", typeof(NewGame), typeof(WizardPage), new PropertyMetadata(new NewGame()));

        public virtual void OnLeavingPage() {

        }

        public virtual void OnEnteringPage() {

        }

        public virtual void OnForward(object sender, RoutedEventArgs args) {

        }

        public virtual void OnBackward(object sender, RoutedEventArgs args) {

        }
    }
}
