namespace Octide.ViewModel
{
    using GalaSoft.MvvmLight;

    public class MainViewModel : ViewModelBase
    {
        public string Title
        {
            get
            {
                return "OCTIDE - ";
            }
        }

        public MainViewModel()
        {
            
        }
    }
}