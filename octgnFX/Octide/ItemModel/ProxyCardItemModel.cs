using GalaSoft.MvvmLight;
using Octgn.ProxyGenerator.Definitions;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class ProxyTextBlockItemModel : ViewModelBase
    {
        public ProxyTextItemModel TextBlock { get; private set; }
        private string _property;

        public ProxyTextBlockItemModel()
        {
        }

        public ProxyTextBlockItemModel(LinkDefinition ld)
        {
            TextBlock = ViewModelLocator.ProxyTabViewModel.TextBlocks.First(x => x.Id == ld.Block);
            _property = ld.NestedProperties.First().Name;
        }

        public object Text
        {
            get
            {
                if (_property == "Name")
                {
                    return ViewModelLocator.ProxyCardViewModel.Card.Name;
                }
                else
                {
                    return ViewModelLocator.ProxyCardViewModel.Card.Properties.First(x => x.Name == _property).Value;
                }
            }
            set
            {
                if (_property == "Name")
                {
                    var prop = ViewModelLocator.ProxyCardViewModel.Card.Name;
                    if (prop == value.ToString()) return;
                    prop = value.ToString();
                }
                else
                {
                    var prop = ViewModelLocator.ProxyCardViewModel.Card.Properties.First(x => x.Name == _property);
                    if (prop.Value == value) return;
                    prop.Value = value;
                }
                RaisePropertyChanged("Text");
            }

        }
    }
}
