using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Octgn.DataNew.Entities;

namespace Octide.ViewModel
{
    public class AssetsTabViewModel : ViewModelBase
    {
        public ObservableCollection<AssetTreeViewItemViewModel> TreeViewItems
        {
            get
            {
                return this.treeViewItems;
            }
            set
            {
                if (value.Equals(this.treeViewItems)) return;
                this.treeViewItems = value;
                this.RaisePropertyChanged("TreeViewItems");
            }
        }

        private ObservableCollection<AssetTreeViewItemViewModel> treeViewItems;

        public AssetsTabViewModel()
        {
			TreeViewItems = new ObservableCollection<AssetTreeViewItemViewModel>();
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this, x => this.RefreshValues());
        }

        public void LoadAsset()
        {
            
        }

        internal void RefreshValues()
        {
            if (!DispatcherHelper.UIDispatcher.CheckAccess())
            {
                DispatcherHelper.RunAsync(RefreshValues);
                return;
            }
            if (!ViewModelLocator.GameLoader.ValidGame)
            {
				TreeViewItems.Clear();
                return;
            }

            var path = ViewModelLocator.GameLoader.GamePath;
            var di = new DirectoryInfo(path);

            foreach (var d in di.GetDirectories().OrderBy(x=>x.Name))
            {
                if (!object.Equals((d.Attributes & FileAttributes.System), FileAttributes.System) &&
                    !object.Equals((d.Attributes & FileAttributes.Hidden), FileAttributes.Hidden))
                {
                    TreeViewItems.Add(new AssetTreeViewItemViewModel(d));
                }
            }

            foreach (var f in di.GetFiles().OrderBy(x => x.Name))
            {
                TreeViewItems.Add(new AssetTreeViewItemViewModel(f));
            }
        }
    }

    public class AssetTreeViewItemViewModel : ViewModelBase
    {
        public ObservableCollection<AssetTreeViewItemViewModel> Children
        {
            get
            {
                return this.children;
            }
            set
            {
                if (value.Equals(this.children)) return;
                this.children = value;
                this.RaisePropertyChanged("Children");
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (value.Equals(this.isExpanded)) return;
                this.isExpanded = value;
                this.RaisePropertyChanged("IsExpanded");
				this.RaisePropertyChanged("ImageSource");
            }
        }

        public FileSystemInfo FileSystemInfo
        {
            get
            {
                return this.fileSystemInfo;
            }
            set
            {
                if (value.Equals(this.fileSystemInfo)) return;
                this.fileSystemInfo = value;
                this.RaisePropertyChanged("FileSystemInfo");
				this.RaisePropertyChanged("ImageSource");
            }
        }

        public ImageSource ImageSource
        {
            get
            {
                if (FileSystemInfo is DirectoryInfo)
                {
                    return FolderManager.GetImageSource(this.FileSystemInfo.FullName, IsExpanded ? ItemState.Open : ItemState.Close);
                }
                else
                {
                    return FileManager.GetImageSource(FileSystemInfo.FullName);
                }
            }
        }

        private ImageSource imageSource;

        private FileSystemInfo fileSystemInfo;

        private bool isExpanded;
		
        private ObservableCollection<AssetTreeViewItemViewModel> children;

        public AssetTreeViewItemViewModel(FileSystemInfo info)
        {
			Children = new ObservableCollection<AssetTreeViewItemViewModel>();
            FileSystemInfo = info;

            if (info is DirectoryInfo)
            {
                var di = info as DirectoryInfo;
                foreach (var d in di.GetDirectories().OrderBy(x => x.Name))
                {
                    if (!object.Equals((d.Attributes & FileAttributes.System), FileAttributes.System) &&
                        !object.Equals((d.Attributes & FileAttributes.Hidden), FileAttributes.Hidden))
                    {
                        Children.Add(new AssetTreeViewItemViewModel(d));
                    }
                }

                foreach (var f in di.GetFiles().OrderBy(x => x.Name))
                {
                    Children.Add(new AssetTreeViewItemViewModel(f));
                }
            }
        }
    }
}