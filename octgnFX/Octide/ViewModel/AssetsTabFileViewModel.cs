using GalaSoft.MvvmLight;

namespace Octide.ViewModel
{
    public abstract class AssetsTabFileViewModelBase : ViewModelBase
    {
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (value == displayName) return;
                displayName = value;
                RaisePropertyChanged("DisplayName");
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                if (value == fileName) return;
                fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        private string fileName;

        private string displayName;

		// Load files into array on start, and have tab control item source
		// refelect current loaded ones or whatever...
    }
}