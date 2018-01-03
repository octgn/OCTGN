using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using Octgn.Core.DataExtensionMethods;
using Octide.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Octgn.Library;
using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace Octide.ViewModel
{
    public class SetSummaryViewModel : ViewModelBase
    {
        public SetSummaryViewModel()
        {
        }

        private SetItemModel _set;


        public SetItemModel Set
        {
            get
            {
                return _set;
            }
            set
            {
                if (_set == value) return;
                _set = value;
                RaisePropertyChanged("Set");
            }
        }
    }
}