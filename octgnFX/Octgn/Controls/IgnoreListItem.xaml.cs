using System;
using Octgn.Controls.ControlTemplates;
using Octgn.Online;

namespace Octgn.Controls
{
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
            if (other.User.Status == UserStatus.Online)
            {
                if (this.User.Status == UserStatus.Online)
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