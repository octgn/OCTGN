using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Octgn.DataNew.Entities;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class AssetsTabViewModel : ViewModelBase
    {
        public AssetsTabViewModel()
        {
            var _game = ViewModelLocator.GameLoader.Game;
        }
    }
}