using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using FontFamily = System.Windows.Media.FontFamily;
using System.IO;
using System.Drawing;
using Font = Octgn.DataNew.Entities.Font;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octide.Messages;
using Octgn.DataNew.Entities;
using Octgn.ProxyGenerator;
using Octgn.Extentions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octgn.ProxyGenerator.Definitions;
using System.Windows.Controls;
using Octgn.Core.DataExtensionMethods;
using System.Drawing.Text;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class ProxyTabViewModel : ViewModelBase
    {

        private Visibility _panelVisibility;
        private ProxyDefinition _proxydef;
        private ProxyTemplateItemModel _selectedItem;
        public ObservableCollection<ProxyTemplateItemModel> Templates { get; private set; }
        public ObservableCollection<ProxyTextItemModel> TextBlocks { get; private set; }
        public ObservableCollection<ProxyOverlayItemModel> OverlayBlocks { get; private set; }

        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand AddOverlayCommand { get; private set; }


        public ProxyTabViewModel()
        {
            _proxydef = ViewModelLocator.GameLoader.ProxyDef;
            PanelVisibility = Visibility.Collapsed;
            Templates = new ObservableCollection<ProxyTemplateItemModel>(_proxydef.TemplateSelector.GetTemplates().Select(x => new ProxyTemplateItemModel(x)));
            Templates.CollectionChanged += (a, b) =>
            {
                _proxydef.TemplateSelector.ClearTemplates();
                foreach (var x in Templates)
                {
                    _proxydef.TemplateSelector.AddTemplate(x._def);
                }
            };

            TextBlocks = new ObservableCollection<ProxyTextItemModel>(_proxydef.BlockManager.GetBlocks().Where(x => x.type == "text").Select(x => new ProxyTextItemModel(x)));
            TextBlocks.CollectionChanged += (a, b) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (var x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (var x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            OverlayBlocks = new ObservableCollection<ProxyOverlayItemModel>(_proxydef.BlockManager.GetBlocks().Where(x => x.type == "overlay").Select(x => new ProxyOverlayItemModel(x)));
            OverlayBlocks.CollectionChanged += (a, b) =>
            {
                _proxydef.BlockManager.ClearBlocks();
                foreach (var x in TextBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
                foreach (var x in OverlayBlocks)
                {
                    _proxydef.BlockManager.AddBlock(x._def);
                }
            };

            AddCommand = new RelayCommand(AddTemplate);
            RemoveCommand = new RelayCommand(RemoveTemplate, EnableButton);
            AddOverlayCommand = new RelayCommand(AddOverlay);
        }

        public ProxyTemplateItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                if (value == null) PanelVisibility = Visibility.Collapsed;
                else PanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
                RemoveCommand.RaiseCanExecuteChanged();
            }
        }
        
        public Visibility PanelVisibility
        {
            get { return _panelVisibility; }
            set
            {
                if (value == _panelVisibility) return;
                _panelVisibility = value;
                RaisePropertyChanged("PanelVisibility");
            }
        }

        public bool EnableButton()
        {
            return _selectedItem != null;
        }

        public void AddTemplate()
        {
            var ret = new ProxyTemplateItemModel() { Name = "Template" };
            Templates.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveTemplate()
        {
            Templates.Remove(SelectedItem);
        }

        public void AddOverlay()
        {
            var ret = new ProxyOverlayItemModel();
            OverlayBlocks.Add(ret);
            RaisePropertyChanged("OverlayBlocks");
        }
    }


}