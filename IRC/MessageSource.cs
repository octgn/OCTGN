using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcDotNet;
using System.Windows.Controls;
using System.ComponentModel;

namespace Octgn.IRC
{
  class MessageSource: INotifyPropertyChanged
  {
    private readonly IIrcMessageTarget ircTarget;
    private bool _isDirty = false;

    public IIrcMessageTarget IrcTarget { get { return ircTarget; } }

    public string Name { get { return ircTarget.Name; } }
    public bool IsDirty
    {
      get { return _isDirty; }
      set
      {
        if (value == _isDirty) return;
        _isDirty = value;
        OnPropertyChanged("IsDirty");
      }
    }

    public RichTextBox OutputBox { get; set; }

    public MessageSource(IIrcMessageTarget target)
    {
      ircTarget = target;
    }

    public event PropertyChangedEventHandler  PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null) 
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
