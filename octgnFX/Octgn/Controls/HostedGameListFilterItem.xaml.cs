using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
