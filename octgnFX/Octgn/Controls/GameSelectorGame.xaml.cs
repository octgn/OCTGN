using System.Windows.Controls;

namespace Octgn.Controls
{
    using System.Windows;

    using Octgn.ViewModels;

    public partial class GameSelectorGame : UserControl
    {
        public static DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(DataGameViewModel), typeof(GameSelectorGame));

        public DataGameViewModel Model
        {
            get{return this.GetValue(ModelProperty) as DataGameViewModel;}
            set{SetValue(ModelProperty,value);}
        }

        public GameSelectorGame()
        {
            InitializeComponent();
        }
    }
}
