namespace Octgn.Core.Tabletop
{
    using System;
    using System.ComponentModel;

    using Octgn.Core.Annotations;
    using Octgn.DataNew.Entities;

    public class GameEngine : INotifyPropertyChanged
    {
        public Game Definition { get; internal set; }

        public GameEngine(Game game)
        {
            Definition = game;
        } 

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}