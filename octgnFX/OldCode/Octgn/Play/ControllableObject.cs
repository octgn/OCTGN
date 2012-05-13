using System;
using System.ComponentModel;
using System.Diagnostics;
using Octgn.Controls;

namespace Octgn.Play
{
    public abstract class ControllableObject : INotifyPropertyChanged
    {
        #region Private fields

        private readonly Player _owner;
        private Player _controller; // The player who controlls this object (null for the table)
        private byte _keepControl; // > 0 if this object should not be passed to someone else

        #endregion

        #region Public interface

        // Id of this object 
        protected ControllableObject(Player owner)
        {
            _owner = Controller = owner;
            _keepControl = 0;
        }

        internal abstract int Id { get; }

        // Name of this object 
        public abstract string Name { get; }

        // Name of this object, including owner
        public virtual string FullName
        {
            get { return Owner == null ? Name : String.Format("{0}'s {1}", Owner.Name, Name); }
        }

        // Player who owns this object (if any)
        public Player Owner
        {
            get { return _owner; }
        }

        // Get the controller of this object
        public Player Controller
        {
            get { return _controller; }
            internal set
            {
                if (_controller == value) return;
                _controller = value;
                OnControllerChanged();
                OnPropertyChanged("Controller");
            }
        }

        public static ControllableObject Find(int id)
        {
            switch ((byte) (id >> 24))
            {
                case 0:
                    return Card.Find(id);
                case 1:
                    return Group.Find(id);
                case 2:
                    //TODO: make counters controllable objects    
                    //return Counter.Find(id);
                    return null;
                default:
                    return null;
            }
        }

        // C'tor

        // Pass control to player p exclusively
        public void PassControlTo(Player p)
        {
            PassControlTo(p, Player.LocalPlayer, true, false);
        }

        public void TakeControl()
        {
            if (Controller == Player.LocalPlayer) return;
            Program.Client.Rpc.TakeFromReq(this, Controller);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        internal void TakingControl(Player p)
        {
            if (_keepControl > 0)
                Program.Client.Rpc.DontTakeReq(this, p);
            else
                PassControlTo(p, Player.LocalPlayer, true, true);
        }

        // Pass control to player p
        internal void PassControlTo(Player p, Player who, bool notifyServer, bool requested)
        {
            if (notifyServer)
            {
                // Can't pass control if I don't own it
                if (Controller != Player.LocalPlayer) return;
                Program.Client.Rpc.PassToReq(this, p, requested);
            }
            Controller = p;
            if (requested)
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(p),
                                         "{0} takes control of {1}", p, this);
            else
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                         "{0} gives control of {1} to {2}", who, this, p);
        }

        // Prevents others from acquiring control of this object
        internal void KeepControl()
        {
            _keepControl++;
        }

        // Allow others to take control of this object
        internal void ReleaseControl()
        {
            if (_keepControl > 0) _keepControl--;
            else
                Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame,
                                         "[ReleaseControl] Called with no matching call to KeepControl().");
        }

        // Return true if the object can be manipulated by the local player
        internal virtual bool CanManipulate()
        {
            return Controller == Player.LocalPlayer || Controller == null;
        }

        // Return true if we can manipulate this object, otherwise display an error and return false
        internal virtual bool TryToManipulate()
        {
            if (CanManipulate())
                return true;
            NotControlledError();
            return false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)//test to see if the handler is hooked
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Error (internal)

        // Show an "Not controlled by you" error tooltip
        internal abstract void NotControlledError();

        // Show an "Controlship was refused" error tooltip
        internal void DontTakeError()
        {
            Tooltip.PopupError(string.Format("Controlship of {0} was refused to you.", FullName));
        }

        #endregion

        #region Controllers (internal)

        protected virtual void OnControllerChanged()
        {
        }

        // Give to the parameter the same controller as this object
        internal void CopyControllersTo(ControllableObject other)
        {
            if (Controller == null) return;
            other.Controller = Controller;
        }

        #endregion
    }
}