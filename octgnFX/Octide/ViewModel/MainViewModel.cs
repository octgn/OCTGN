namespace Octide.ViewModel
{
    using GalaSoft.MvvmLight;

    public class MainViewModel : ViewModelBase
    {
        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (value == this.title) return;
                this.title = value;
				RaisePropertyChanged(this.Title);
            }
        }

        public MainViewModel()
        {
            Title = "OCTIDE";
        }
    }
}