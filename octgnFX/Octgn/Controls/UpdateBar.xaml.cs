using System.ComponentModel;
using System.Windows.Controls;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for UpdateBar.xaml
    /// </summary>
    public partial class UpdateBar : INotifyPropertyChanged 
    {
        public string Message
        {
            get
            {
                return "Your version of OCTGN is out of date.";
            }
        }
        public UpdateBar()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
