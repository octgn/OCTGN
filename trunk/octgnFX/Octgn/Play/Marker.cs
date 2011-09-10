using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Octgn.Play
{
	public class Marker : INotifyPropertyChanged
	{
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
				if (value < _count)
					Program.Client.Rpc.RemoveMarkerReq(card, Model.id, Model.Name, (ushort)(_count - value));
				else if (value > _count)
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
			if (value == 0)
				card.RemoveMarker(this);
			else if (value != _count)
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
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}
}