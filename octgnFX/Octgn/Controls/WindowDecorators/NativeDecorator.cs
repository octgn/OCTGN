using System;
using System.ComponentModel;
using System.Windows;

namespace Octgn.Controls.WindowDecorators
{
    class NativeDecorator : WindowDecorator
    {
        public NativeDecorator(DecorableWindow decorated) : base(decorated)
        {
            IsUndoable = true;
        }

        public override void Apply()
        {
            Decorated.PropertyChanged += DecoratedOnPropertyChanged;
        }

        public override bool Undo()
        {
            Decorated.PropertyChanged -= DecoratedOnPropertyChanged;
            return true;
        }

        private void DecoratedOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {

                case "TitleBarVisibility":
                    switch (Decorated.TitleBarVisibility)
                    {
                        case Visibility.Visible:
                            Decorated.WindowStyle = WindowStyle.SingleBorderWindow;
                            break;
                        case Visibility.Hidden:
                        case Visibility.Collapsed:
                            Decorated.WindowStyle = WindowStyle.None;
                            break;
                    }
                    break;
            }
        }

    }
}
