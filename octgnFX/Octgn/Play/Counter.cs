using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Octgn.Play
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Octgn.Utils;

    public sealed class Counter : INotifyPropertyChanged
    {
        #region Private fields

        private readonly DataNew.Entities.Counter _defintion;
        private readonly byte _id;
        private readonly Player _player; // Player who owns this counter, if any        
        private int _state; // Value of this counter

        #endregion

        #region Public interface

        // Find a counter given its Id

        // Name of this counter
        private readonly string _name;

        public Counter(Player player, DataNew.Entities.Counter def)
        {
            _player = player;
            _state = def.Start;
            _name = def.Name;
            _id = def.Id;
            _defintion = def;
        }

        public Player Owner
        {
            get
            {
                return _player;
            }
        }

        public string Name
        {
            get { return _name; }
        }

        // Get or set the counter's value
        public int Value
        {
            get { return _state; }
            set { SetValue(value, Player.LocalPlayer, true); }
        }

        public DataNew.Entities.Counter Definition
        {
            get { return _defintion; }
        }

        public static Counter Find(int id)
        {
            Player p = Player.Find((byte) (id >> 16));
            if (p == null || (byte) id > p.Counters.Length || (byte) id == 0)
                return null;
            return p.Counters[(byte) id - 1];
        }

        // C'tor

        public override string ToString()
        {
            return (_player != null ? _player.Name + "'s " : "Global ") + Name;
        }

        #endregion

        #region Implementation



        // Get the id of this counter
        internal int Id
        {
            get { return 0x02000000 | (_player == null ? 0 : _player.Id << 16) | _id; }
        }

        private readonly CompoundCall setCounterNetworkCompoundCall = new CompoundCall();

        // Set the counter's value
        internal void SetValue(int value, Player who, bool notifyServer)
        {
            var oldValue = _state;
            // Check the difference with current value
            int delta = value - _state;
            if (delta == 0) return;
            // Notify the server if needed
            if (notifyServer)
            {
                setCounterNetworkCompoundCall.Call(() => Program.Client.Rpc.CounterReq(this, value));
            }
            // Set the new value
            _state = value;
            OnPropertyChanged("Value");
            // Display a notification in the chat
            string deltaString = (delta > 0 ? "+" : "") + delta.ToString(CultureInfo.InvariantCulture);
            if (notifyServer || who != Player.LocalPlayer)
                Program.GameEngine.EventProxy.OnChangeCounter(who, this, oldValue);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} sets {1} counter to {2} ({3})", who, this, value, deltaString);
        }

        internal void Reset()
        {
            if (!Definition.Reset) return;
            _state = Definition.Start;
            OnPropertyChanged("Value");
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}