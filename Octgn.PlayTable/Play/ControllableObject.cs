using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Octgn.Play
{
    using Octgn.Core;
    using Octgn.Core.Play;
    using Octgn.Networking;
    using Octgn.PlayTable.Controls;

    public abstract class ControllableObject : INotifyPropertyChanged, IPlayControllableObject
    {
        #region Private fields

        private readonly IPlayPlayer _owner;
        private IPlayPlayer _controller; // The player who controlls this object (null for the table)
        private byte _keepControl; // > 0 if this object should not be passed to someone else

        #endregion

        #region Public interface

        // Id of this object 
        protected ControllableObject(IPlayPlayer owner)
        {
            _owner = Controller = owner;
            _keepControl = 0;
        }

        public abstract int Id { get; }

        // Name of this object 
        public abstract string Name { get; }

        // Name of this object, including owner
        public virtual string FullName
        {
            get { return Owner == null ? Name : String.Format("{0}'s {1}", Owner.Name, Name); }
        }

        // Player who owns this object (if any)
        public IPlayPlayer Owner
        {
            get { return _owner; }
        }

        // Get the controller of this object
        public IPlayPlayer Controller
        {
            get { return _controller; }
            set
            {
                if (_controller == value) return;
                _controller = value;
                OnControllerChanged();
                OnPropertyChanged("Controller");
            }
        }

        // C'tor

        // Pass control to player p exclusively
        public void PassControlTo(IPlayPlayer p)
        {
            PassControlTo(p, K.C.Get<PlayerStateMachine>().LocalPlayer, true, false);
        }

        public void TakeControl()
        {
            if (Controller == K.C.Get<PlayerStateMachine>().LocalPlayer) return;
            K.C.Get<Client>().Rpc.TakeFromReq(this, Controller);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void TakingControl(IPlayPlayer p)
        {
            if (_keepControl > 0)
                K.C.Get<Client>().Rpc.DontTakeReq(this, p);
            else
                PassControlTo(p, K.C.Get<PlayerStateMachine>().LocalPlayer, true, true);
        }

        // Pass control to player p
        public void PassControlTo(IPlayPlayer p, IPlayPlayer who, bool notifyServer, bool requested)
        {
            if (notifyServer)
            {
                // Can't pass control if I don't own it
                if (Controller != K.C.Get<PlayerStateMachine>().LocalPlayer) return;
                K.C.Get<Client>().Rpc.PassToReq(this, p, requested);
            }
            Controller = p;
            if (requested)
                K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(p),
                                         "{0} takes control of {1}", p, this);
            else
                K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                         "{0} gives control of {1} to {2}", who, this, p);
        }

        // Prevents others from acquiring control of this object
        public void KeepControl()
        {
            _keepControl++;
        }

        // Allow others to take control of this object
        public void ReleaseControl()
        {
            if (_keepControl > 0) _keepControl--;
            else
                K.C.Get<GameplayTrace>().TraceEvent(TraceEventType.Error, EventIds.NonGame,
                                         "[ReleaseControl] Called with no matching call to KeepControl().");
        }

        // Return true if the object can be manipulated by the local player
        public virtual bool CanManipulate()
        {
            return Controller == K.C.Get<PlayerStateMachine>().LocalPlayer || Controller == null;
        }

        // Return true if we can manipulate this object, otherwise display an error and return false
        public virtual bool TryToManipulate()
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
        public abstract void NotControlledError();

        // Show an "Controlship was refused" error tooltip
        public void DontTakeError()
        {
            Tooltip.PopupError(string.Format("Controlship of {0} was refused to you.", FullName));
        }

        #endregion

        #region Controllers (internal)

        public virtual void OnControllerChanged()
        {
        }

        // Give to the parameter the same controller as this object
        public void CopyControllersTo(IPlayControllableObject other)
        {
            if (Controller == null) return;
            other.Controller = Controller;
        }

        #endregion
    }
}