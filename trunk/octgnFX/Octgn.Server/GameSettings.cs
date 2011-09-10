using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Octgn.Play
{
	public class GameSettings : INotifyPropertyChanged
	{
		private bool _useTwoSidedTable;

		public bool UseTwoSidedTable
		{ 
			get { return _useTwoSidedTable; }
			set
			{
				if (value != _useTwoSidedTable)
				{
					_useTwoSidedTable = value;
					OnPropertyChanged("UseTwoSidedTable");
				}
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
