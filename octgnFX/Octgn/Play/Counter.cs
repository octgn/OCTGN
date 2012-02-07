using System;
using System.ComponentModel;
using System.Diagnostics;
using Octgn.Definitions;
using Octgn.Play.Gui;


namespace Octgn.Play
{
    public sealed class Counter : INotifyPropertyChanged
    {
        #region Private fields

        private CounterDef defintion;
        private Player player;      // Player who owns this counter, if any        
        private int state;          // Value of this counter
        private byte id;

        #endregion

        #region Public interface

        // Find a counter given its Id
        public static Counter Find(int id)
        {
            Player p = Player.Find((byte)(id >> 16));
            if (p == null || (byte)id > p.Counters.Length || (byte)id == 0)
                return null;
            return p.Counters[(byte)id - 1];
        }

        // Name of this counter
        private readonly string _name;
        public string Name
        { get { return _name; } }

        // Get or set the counter's value
        public int Value
        {
            get
            { return state; }
            set
            { SetValue(value, Player.LocalPlayer, true); }
        }

        public CounterDef Definition
        { get { return defintion; } }

        // C'tor
        public Counter(Player player, CounterDef def)
        {
            this.player = player;
            state = def.Start; _name = def.Name; id = def.Id;
            defintion = def;
        }

        public override string ToString()
        {
            return (player != null ? player.Name + "'s " : "Global ") + Name;
        }

        #endregion

        #region Implementation

        // Get the id of this counter
        internal int Id
        {
            get
            {
                return 0x02000000 | (player == null ? 0 : player.Id << 16) | id;
            }
        }

        // Set the counter's value
        internal void SetValue(int value, Player who, bool notifyServer)
        {
            // Check the difference with current value
            int delta = value - state;
            if (delta == 0) return;
            // Notify the server if needed
            if (notifyServer)
                Program.Client.Rpc.CounterReq(this, value);
            // Set the new value
            state = value; OnPropertyChanged("Value");
            // Display a notification in the chat
            string deltaString = (delta > 0 ? "+" : "") + delta.ToString();
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who), "{0} sets {1} counter to {2} ({3})", who, this, value, deltaString);
        }

        internal void Reset()
        {
            if (!Definition.Reset) return;
            state = Definition.Start; OnPropertyChanged("Value");
        }

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
}
