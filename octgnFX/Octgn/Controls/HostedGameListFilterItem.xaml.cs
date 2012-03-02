using System;
using Microsoft.Windows.Controls.Ribbon;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for HostedGameListFilterItem.xaml
    /// </summary>
    public partial class HostedGameListFilterItem : RibbonCheckBox
    {
        public Guid _game;

        public Guid GameId
        {
            get { return _game; }
            set
            {
                _game = value;       
            }
        }
        public HostedGameListFilterItem()
        {
            InitializeComponent();
        }      
    }
}
