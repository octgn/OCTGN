namespace Octgn.Play.Gui.Messages
{
    public abstract class NotifyPropertyChanged
    {
        public string Name { get; set; }

        protected NotifyPropertyChanged(string name)
        {
            Name = name;
        }
    }

    public class GroupNotifyPropertyChanged
    {
        
    }
}