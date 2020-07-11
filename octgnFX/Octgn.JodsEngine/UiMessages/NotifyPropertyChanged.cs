using Octgn.Play;

namespace Octgn.UiMessages
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

    public class PlayerGlobalVariableChanged
    {
        public Player Player{get;set;}

        public PlayerGlobalVariableChanged(Player p)
        {
            Player = p;
        }
    }

    public class RefreshSharedDecksMessage
    {
        
    }
}