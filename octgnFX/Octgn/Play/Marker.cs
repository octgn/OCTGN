using System;
using System.ComponentModel;
using Octgn.Data;

namespace Octgn.Play
{
    using System.Threading;
    using System.Threading.Tasks;

    using Octgn.Utils;

    public class Marker : INotifyPropertyChanged
    {
        internal static readonly DefaultMarkerModel[] DefaultMarkers = new[]
                                                                           {
                                                                               new DefaultMarkerModel("white",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 1)),
                                                                               new DefaultMarkerModel("blue",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 2)),
                                                                               new DefaultMarkerModel("black",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 3)),
                                                                               new DefaultMarkerModel("red",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 4)),
                                                                               new DefaultMarkerModel("green",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 5)),
                                                                               new DefaultMarkerModel("orange",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 6)),
                                                                               new DefaultMarkerModel("brown",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 7)),
                                                                               new DefaultMarkerModel("yellow",
                                                                                                      new Guid(0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 0, 0,
                                                                                                               0, 8))
                                                                           };

        private readonly Card _card;
        private readonly DataNew.Entities.Marker _model;
        private ushort _count = 1;

        public Marker(Card card, DataNew.Entities.Marker model)
        {
            _card = card;
            _model = model;
        }

        public Marker(Card card, DataNew.Entities.Marker model, ushort count)
            : this(card, model)
        {
            _count = count;
        }

        public DataNew.Entities.Marker Model
        {
            get { return _model; }
        }

        // private readonly CompoundCall setCountNetworkCompoundCall = new CompoundCall();

        public ushort Count
        {
            get { return _count; }
            set
            {
                int count = _count;
				//setCountNetworkCompoundCall.Call(()=>
				//{
				    var val = value;
                    if (val < count)
                        Program.Client.Rpc.RemoveMarkerReq(_card, Model.Id, Model.Name, (ushort)(count - val));
                    else if (val > count)
                        Program.Client.Rpc.AddMarkerReq(_card, Model.Id, Model.Name, (ushort)(val - count));
                //});
                if (value == _count) return;
                SetCount(value);
            }
        }

        public string Description
        {
            get { return Model.Name + " (x" + Count + ")"; }
        }

        public Card Card
        {
            get { return _card; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public override string ToString()
        {
            return Model.Name;
        }

        internal void SetCount(ushort value)
        {
            if (value == 0)
                _card.RemoveMarker(this);
            else if (value != _count)
            {
                _count = value;
                OnPropertyChanged("Count");
                OnPropertyChanged("Description");
            }
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}