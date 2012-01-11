using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace Octgn.Play
{
  public abstract class ControllableObject : INotifyPropertyChanged
  {
    #region Private fields

    private Player owner;
    private Player controller;      // The player who controlls this object (null for the table)
    private byte keepControl;       // > 0 if this object should not be passed to someone else

    #endregion

    #region Public interface

    public static ControllableObject Find(int id)
    {
      switch ((byte)(id >> 24))
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

    // Id of this object 
    internal abstract int Id
    { get; }

    // Name of this object 
    public abstract string Name
    { get; }

    // Name of this object, including owner
    public virtual string FullName
    { get { return Owner == null ? Name : String.Format("{0}'s {1}", Owner.Name, Name); } }

    // Player who owns this object (if any)
    public Player Owner
    {
      get { return owner; }
    }

    // Get the controller of this object
    public Player Controller
    {
      get { return controller; }
      internal set
      {
        if (controller != value)
        {
          controller = value;
          OnControllerChanged();
          OnPropertyChanged("Controller");
        }
      }
    }

    // C'tor
    public ControllableObject(Player owner)
    {
      this.owner = Controller = owner;
      keepControl = 0;
    }

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

    internal void TakingControl(Player p)
    {
      if (keepControl > 0)
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
        Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(p), "{0} takes control of {1}", p, this);
      else
        Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who), "{0} gives control of {1} to {2}", who, this, p);
    }

    #region Controllers (internal)

    protected virtual void OnControllerChanged()
    { }

    // Give to the parameter the same controller as this object
    internal void CopyControllersTo(ControllableObject other)
    {
      if (Controller == null) return;
      other.Controller = Controller;
    }

    #endregion

    // Prevents others from acquiring control of this object
    internal void KeepControl()
    { keepControl++; }

    // Allow others to take control of this object
    internal void ReleaseControl()
    {
      if (keepControl > 0) keepControl--;
      else Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "[ReleaseControl] Called with no matching call to KeepControl().");
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
      else
      {
        NotControlledError();
        return false;
      }
    }

    #region Error (internal)

    // Show an "Not controlled by you" error tooltip
    internal abstract void NotControlledError();

    // Show an "Controlship was refused" error tooltip
    internal void DontTakeError()
    {
      Controls.Tooltip.PopupError(string.Format("Controlship of {0} was refused to you.", FullName));
    }

    #endregion

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
