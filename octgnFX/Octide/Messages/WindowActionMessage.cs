namespace Octide.Messages
{
    using GalaSoft.MvvmLight;

    using Microsoft.Practices.ServiceLocation;

    public class WindowActionMessage<T> where T : ViewModelBase
    {
        public WindowActionType Action { get; internal set; }
        public T WindowViewModel { get; internal set; }

        public WindowActionMessage(WindowActionType action)
        {
            Action = action;
            var a = typeof(T);
            WindowViewModel = ServiceLocator.Current.GetInstance<T>();
        }
    }

    public enum WindowActionType
    {
        Close,Show,Create,SetMain
    }
}