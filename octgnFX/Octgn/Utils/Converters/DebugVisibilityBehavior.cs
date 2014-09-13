using System.Windows;
using System.Windows.Interactivity;
using Octgn.Library;

namespace Octgn.Utils.Converters
{
    public class DebugVisibilityBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty HideIfDebugProperty = DependencyProperty.Register(
            "HideIfDebug", typeof (bool), typeof (DebugVisibilityBehavior), new PropertyMetadata(default(bool)));

        public bool HideIfDebug
        {
            get { return (bool) GetValue(HideIfDebugProperty); }
            set { SetValue(HideIfDebugProperty, value); }
        }

        public static readonly DependencyProperty HideVisibilityProperty = DependencyProperty.Register(
            "HideVisibility", typeof (Visibility), typeof (DebugVisibilityBehavior), new PropertyMetadata(default(Visibility)));

        public Visibility HideVisibility
        {
            get { return (Visibility) GetValue(HideVisibilityProperty); }
            set { SetValue(HideVisibilityProperty, value); }
        }

        public DebugVisibilityBehavior()
        {
            HideVisibility = Visibility.Collapsed;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Visibility = HideIfDebug == false
                ? (X.Instance.Debug ? Visibility.Visible : HideVisibility)
                : (X.Instance.Debug ? HideVisibility : Visibility.Visible);
        }
    }
}