using System;
using System.ComponentModel;

namespace Octgn.Play
{
    public class Marker : INotifyPropertyChanged
    {
        internal readonly static DefaultMarkerModel[] DefaultMarkers = new DefaultMarkerModel[]
        {
            new DefaultMarkerModel("white", new Guid(0,0,0,0,0,0,0,0,0,0,1)),
            new DefaultMarkerModel("blue",  new Guid(0,0,0,0,0,0,0,0,0,0,2)),
            new DefaultMarkerModel("black", new Guid(0,0,0,0,0,0,0,0,0,0,3)),
            new DefaultMarkerModel("red",  new Guid(0,0,0,0,0,0,0,0,0,0,4)),
            new DefaultMarkerModel("green", new Guid(0,0,0,0,0,0,0,0,0,0,5)),
            new DefaultMarkerModel("orange", new Guid(0,0,0,0,0,0,0,0,0,0,6)),
            new DefaultMarkerModel("brown",  new Guid(0,0,0,0,0,0,0,0,0,0,7)),
            new DefaultMarkerModel("yellow",  new Guid(0,0,0,0,0,0,0,0,0,0,8))
        };
        private readonly Data.MarkerModel _model;
        private ushort _count = 1;
        private Card card;

        public Data.MarkerModel Model
        { get { return _model; } }

        public ushort Count
        {
            get { return _count; }
            set
            {
                if(value < _count)
                    Program.Client.Rpc.RemoveMarkerReq(card, Model.id, Model.Name, (ushort)(_count - value));
                else if(value > _count)
                    Program.Client.Rpc.AddMarkerReq(card, Model.id, Model.Name, (ushort)(value - _count));
                else
                    return;
                SetCount(value);
            }
        }

        public string Description
        {
            get { return Model.Name + " (x" + Count + ")"; }
        }

        public Card Card
        { get { return card; } }

        public Marker(Card card, Data.MarkerModel model)
        { this.card = card; _model = model; }

        public Marker(Card card, Data.MarkerModel model, ushort count)
            : this(card, model)
        { _count = count; }

        public override string ToString()
        { return Model.Name; }

        internal void SetCount(ushort value)
        {
            if(value == 0)
                card.RemoveMarker(this);
            else if(value != _count)
            {
                _count = value;
                OnPropertyChanged("Count");
                OnPropertyChanged("Description");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion INotifyPropertyChanged Members
    }
}