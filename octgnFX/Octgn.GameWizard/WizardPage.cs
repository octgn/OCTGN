using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.GameWizard
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
    }
}
