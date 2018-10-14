using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI
{
    public abstract class PageViewModel : ViewModelBase
    {
        public string Button1Text {
            get => _button1Text;
            set => SetAndNotify(ref _button1Text, value);
        }
        private string _button1Text;

        public bool Button1Visible {
            get => _button1Visible;
            set => SetAndNotify(ref _button1Visible, value);
        }
        private bool _button1Visible = true;

        public UserControl Page {
            get => _page;
            set => SetAndNotify(ref _page, value);
        }
        private UserControl _page;

        public event EventHandler<PageTransitionEventArgs> Transition;

        public virtual void Button1_Action() {

        }

        protected void DoTransition(PageViewModel page) {
            Transition?.Invoke(this, new PageTransitionEventArgs { Page = page });
        }
    }

    public class PageTransitionEventArgs : EventArgs
    {
        public PageViewModel Page { get; set; }
    }
}
