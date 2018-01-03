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
    public class SetCardsViewModel : ViewModelBase
    {
        #region card tab          

        public SetCardsViewModel()
        {
            AddCardCommand = new RelayCommand(AddCard);
            AddAltCommand = new RelayCommand(AddAlt);
        }

        private Visibility _cardPanelVisibility;
        private CardItemModel _selectedItem;
        private SetItemModel _parentSet;
        
        public RelayCommand AddCardCommand { get; private set; }

        public SetItemModel ParentSet
        {
            get
            {
                return _parentSet;
            }
            set
            {
                if (_parentSet == value) return;
                _parentSet = value;
                RaisePropertyChanged("Items");
                SelectedItem = Items?.FirstOrDefault();
            }
        }

        public ObservableCollection<CardItemModel> Items => ParentSet?.CardItems;

        public CardItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;

                CardPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;

                RaisePropertyChanged("SelectedItem");
                if (value != null)
                {
                    SelectedAlt = value.Default;
                }
            }
        }
        
        

        public Visibility CardPanelVisibility
        {
            get { return _cardPanelVisibility; }
            set { Set(ref _cardPanelVisibility, value); }
        }

        public bool EnableCardButton() => SelectedItem != null;

        public void AddCard()
        {
            var ret = new CardItemModel() { Parent = ParentSet };
            Items.Add(ret);
            SelectedItem = ret;
            RaisePropertyChanged("SelectedItem");
        }
        
        #endregion


        #region alt tab

        private AltItemModel _selectedAlt;
        private Visibility _altPanelVisibility;


        public RelayCommand AddAltCommand { get; private set; }

        public Visibility AltPanelVisibility
        {
            get { return _altPanelVisibility; }
            set { Set(ref _altPanelVisibility, value); }
        }

        public AltItemModel SelectedAlt
        {
            get { return _selectedAlt; }
            set
            {
                if (!Set(ref _selectedAlt, value)) return;
                AltPanelVisibility = value == null ? Visibility.Hidden : Visibility.Visible;
                ViewModelLocator.ProxyCardViewModel.Card = value;
                RaisePropertyChanged(nameof(SelectedAlt));
            }
        }

        public void AddAlt()
        {
            var ret = new AltItemModel() { Parent = SelectedItem };
            SelectedItem.Items.Add(ret);
            SelectedAlt = ret;
            RaisePropertyChanged("SelectedAlt");
        }

        #endregion


        #region properties

        public IEnumerable<SizeItemModel> CardSizes => ViewModelLocator.PreviewTabViewModel.CardSizes;

        #endregion
    }

}