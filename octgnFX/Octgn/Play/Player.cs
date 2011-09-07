using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Octgn.Play
{
	public sealed class Player : INotifyPropertyChanged
	{
		#region Static members

		// Contains all players in this game
		private static readonly ObservableCollection<Player> all = new ObservableCollection<Player>();

		// Get all players in the game
		public static IEnumerable<Player> All
		{ get { return all; } }

		// Get all players in the game, except a possible Global player
		public static IEnumerable<Player> AllExceptGlobal
		{
      get { return All.Where(p => p != Player.GlobalPlayer); }
		}

		// Number of players
		internal static int Count
		{
			get
			{
				return GlobalPlayer == null ? all.Count : all.Count - 1;
			}
		}

		// Find a player with his id
		internal static Player Find(byte id)
		{
			foreach (Player p in all)
				if (p.Id == id) return p;
			return null;
		}

		// Resets the player list
		internal static void Reset()
		{ all.Clear(); LocalPlayer = GlobalPlayer = null; }

		// May be null if we're in pure server mode
		public static Player LocalPlayer;
		// May be null if there's no global player in the game definition
		public static Player GlobalPlayer;

		internal static event EventHandler<PlayerEventArgs> PlayerAdded;
		internal static event EventHandler<PlayerEventArgs> PlayerRemoved;

		#endregion

		#region Public fields and properties

		private readonly Counter[] counters;     // Counters this player owns
		public Counter[] Counters
		{
			get { return counters; }
		}

		private readonly Group[] groups;         // Groups this player owns
		public Group[] IndexedGroups
		{
			get { return groups; }
		}

    public IEnumerable<Group> Groups
    {
      get { return groups.Where(g => g != null); }
    }

    public Dictionary<string, int> Variables
    { get; private set; }

		private readonly Hand hand;              // Hand of this player (may be null)
		public Hand Hand
		{ get { return hand; } }

		internal readonly ulong PublicKey;       // Public cryptographic key

		public byte Id                           // Identifier
		{ get; set; }

		private string name;
		public string Name                       // Nickname
		{
			get { return name; }
			set
			{
				if (name != value)
				{
					name = value;
					OnPropertyChanged("Name");
				}
			}
		}

    public bool IsGlobalPlayer
    { get { return Id == 0; } }

		private bool _invertedTable;
		public bool InvertedTable								 // True if the player plays on the opposite side of the table (for two-sided table only)
		{
			get { return _invertedTable; }
			set
			{
				if (_invertedTable != value)
				{
					_invertedTable = value;
					OnPropertyChanged("InvertedTable");
					if (Program.Server != null)					// If we are the host, we are setting this option for everyone
						Program.Client.Rpc.PlayerSettings(this, value);
				}
			}
		}

		private System.Windows.Media.Color color = System.Windows.Media.Colors.Green;
		private System.Windows.Media.Brush solidBrush, transparentBrush;
		// Associated color
		public System.Windows.Media.Color Color
		{
			get
			{
				int idx = all.IndexOf(this);
				// Check if there's a global player
				if (all[0].Id != 0)
					++idx;
				//TODO: return correct color
				//                return System.Windows.Media.ColorConverter.ConvertFromString(Program.settings.GetPlayerColor(idx));
				if ((idx & 1) == 1)
					return System.Windows.Media.Color.FromRgb(0x59, 0xEF, 0x5F);
				else
					return System.Windows.Media.Colors.Red;
			}
		}

		// Work around a WPF binding bug ? Borders don't seem to bind correctly to Color!
		public System.Windows.Media.Brush Brush
		{ get { return solidBrush; } }

		public System.Windows.Media.Brush TransparentBrush
		{ get { return transparentBrush; } }

		#endregion

		#region Public interface

		// C'tor
		internal Player(Definitions.GameDef g, string name, byte id, ulong pkey)
		{
			// Init fields
			this.name = name; this.Id = id; this.PublicKey = pkey;
			// Register the player
			all.Add(this);
			OnPropertyChanged("Color");
			// Create the color brushes            
			solidBrush = new System.Windows.Media.SolidColorBrush(Color);
			solidBrush.Freeze();
			transparentBrush = new System.Windows.Media.SolidColorBrush(Color);
			transparentBrush.Opacity = 0.4;
			transparentBrush.Freeze();
			OnPropertyChanged("Brush");
			OnPropertyChanged("TransparentBrush");			
			// Create counters
			counters = new Counter[g.PlayerDefinition.Counters != null ? g.PlayerDefinition.Counters.Length : 0];
			for (int i = 0; i < Counters.Length; i++)
				Counters[i] = new Counter(this, g.PlayerDefinition.Counters[i]);
      // Create variables
      Variables = new Dictionary<string, int>();
      foreach (var varDef in g.Variables.Where(v => !v.Global))
        Variables.Add(varDef.Name, varDef.DefaultValue);
			// Create a hand, if any
			if (g.PlayerDefinition.Hand != null)
				hand = new Hand(this, g.PlayerDefinition.Hand);
			// Create groups
			groups = new Group[g.PlayerDefinition.Groups != null ? g.PlayerDefinition.Groups.Length + 1 : 1];
			groups[0] = hand;
			for (int i = 1; i < IndexedGroups.Length; i++)
				groups[i] = new Pile(this, g.PlayerDefinition.Groups[i - 1]);
			// Raise the event
			if (PlayerAdded != null) PlayerAdded(null, new PlayerEventArgs(this));
		}

		// C'tor for global items
		internal Player(Definitions.GameDef g)
		{
			var globalDef = g.GlobalDefinition;
			// Register the player
			all.Add(this);
			// Init fields
			name = "Global"; Id = 0; PublicKey = 0;
			// Create counters
			counters = new Counter[globalDef.Counters != null ? globalDef.Counters.Length : 0];
			for (int i = 0; i < Counters.Length; i++)
				Counters[i] = new Counter(this, globalDef.Counters[i]);
			// Create global's player groups
			groups = new Pile[globalDef.Groups != null ? g.GlobalDefinition.Groups.Length + 1 : 0];
			for (int i = 1; i < IndexedGroups.Length; i++)
				groups[i] = new Pile(this, globalDef.Groups[i - 1]);
		}

		// Remove the player from the game
		internal void Delete()
		{
			// Remove from the list
			all.Remove(this);
			// Raise the event
			if (PlayerRemoved != null) PlayerRemoved(null, new PlayerEventArgs(this));
		}

		public override string ToString()
		{ return name; }

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		#endregion
	}

	public class PlayerEventArgs : EventArgs
	{
		public readonly Player Player;

		public PlayerEventArgs(Player p)
		{ this.Player = p; }
	}
}
