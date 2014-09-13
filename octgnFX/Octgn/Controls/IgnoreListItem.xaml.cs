using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    using System.ComponentModel;
    using System.Linq;

    using Octgn.Controls.ControlTemplates;

    /// <summary>
    ///   Interaction logic for IgnoreListItem.xaml
    /// </summary>
    public partial class IgnoreListItem : IComparable<IgnoreListItem>
    {


        public IgnoreListItem()
            : base()
        {
            DataContext = this;
            this.InitializeComponent();
        }

        public IgnoreListItem(User user)
            : base(user)
        {
            DataContext = this;
            InitializeComponent();
        }

        public new int CompareTo(UserListItem other)
        {
            return CompareTo(new IgnoreListItem(other.User));
        }

        public int CompareTo(IgnoreListItem other)
        {
            if (this.User == null) return 1;
            if (other == null || other.User == null) return -1;
            if (this.User.Status == UserStatus.Online)
            {
                if (other.User.Status == UserStatus.Online)
                    return String.Compare(
                        this.User.UserName,
                        other.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.User.Status == UserStatus.Away)
            {
                if (other.User.Status == UserStatus.Online) return 1;
                if (other.User.Status == UserStatus.Away)
                    return String.Compare(
                        this.User.UserName,
                        other.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (this.User.Status == UserStatus.DoNotDisturb)
            {
                if (other.User.Status == UserStatus.Online) return 1;
                if (other.User.Status == UserStatus.Away) return 1;
                if (other.User.Status == UserStatus.DoNotDisturb)
                    return String.Compare(
                        this.User.UserName,
                        other.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return -1;
            }
            if (other.User.Status == UserStatus.Online)
            {
                if (this.User.Status == UserStatus.Online)
                    return String.Compare(
                        other.User.UserName,
                        this.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            if (other.User.Status == UserStatus.Away)
            {
                if (this.User.Status == UserStatus.Online) return -1;
                if (this.User.Status == UserStatus.Away)
                    return String.Compare(
                        other.User.UserName,
                        this.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            if (other.User.Status == UserStatus.DoNotDisturb)
            {
                if (this.User.Status == UserStatus.Online) return -1;
                if (this.User.Status == UserStatus.Away) return -1;
                if (this.User.Status == UserStatus.DoNotDisturb)
                    return String.Compare(
                        other.User.UserName,
                        this.User.UserName,
                        StringComparison.InvariantCultureIgnoreCase);
                return 1;
            }
            return String.Compare(this.User.UserName, other.User.UserName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}