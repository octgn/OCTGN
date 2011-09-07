using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcDotNet;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Windows.Data;

namespace Octgn.IRC
{
  class User : INotifyPropertyChanged
  {
    private readonly IrcChannelUser user;

    public string NickName { get { return user.User.NickName; } }
    public IrcUser IrcUser { get { return user.User; } }

    public User(IrcChannelUser user)
    {
      this.user = user;
      user.User.NickNameChanged += (s, e) => OnPropertyChanged("NickName");
    }

    #region Equality override

    public override bool Equals(object obj)
    {
      var other = obj as User;
      if (other == null) return false;

      return user.Equals(other.user);
    }

    public override int GetHashCode()
    {
      return user.GetHashCode();
    } 

    #endregion

    #region INotifyPropertyChanged
    
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    } 

    #endregion
  }

  class UserCollection : ObservableCollection<User>
  {
    private Dispatcher dispatcher;

    public ICollectionView SortedView { get; private set; }

    public UserCollection(DispatcherObject context, INotifyCollectionChanged model)
    {
      dispatcher = context.Dispatcher;
      model.CollectionChanged += SynchronizeCollections;      
      context.Dispatcher.Invoke(new Action(()  =>
      {
        SortedView = new ListCollectionView(this) { SortDescriptions = { new SortDescription("NickName", ListSortDirection.Ascending) } };
      }));
    }

    private void SynchronizeCollections(object sender, NotifyCollectionChangedEventArgs e)
    {
      dispatcher.BeginInvoke(new Action(() =>
      {
        if (e.OldItems != null)
          foreach (var item in e.OldItems.Cast<IrcChannelUser>())
            this.Remove(new User(item));

        if (e.NewItems != null)
          foreach (var item in e.NewItems.Cast<IrcChannelUser>())
            this.Add(new User(item));
      }));
    }
  }
}
