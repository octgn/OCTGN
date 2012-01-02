using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Skylabs.Lobby
{
    [Serializable]
    public enum UserLevel { Standard = 0, Moderator = 1, Admin = 2 };

    [Serializable]
    public enum UserStatus { Unknown = 0, Offline = 1, Online = 2, Away = 3, DoNotDisturb = 4, Invisible = 5 };

    [Serializable]
    public class User : IEquatable<User>, ICloneable
    {
        public int Uid { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }

        public string CustomStatus { get; set; }

        public UserStatus Status { get; set; }

        public UserLevel Level { get; set; }

        public User()
        {
            Uid = -1;
            Email = "";
            Password = "";
            DisplayName = "";
            CustomStatus = "";
            Status = UserStatus.Unknown;
            Level = UserLevel.Standard;
        }

        public bool Equals(User u)
        {
            if (u == null)
            {
                //TODO Something Causes u to be null, need to figger it out.
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                return false;
            }
            return (Uid == u.Uid);
        }

        public byte[] Serialize()
        {
            using(MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                ms.Flush();
                return ms.ToArray();
            }
        }

        public static User Deserialize(byte[] data)
        {
            using(MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (User)bf.Deserialize(ms);
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
        #region ICloneable Members

        public object Clone()
        {
            User ret = new User
                           {Email = Email, Uid = Uid, Password = Password, Level = Level, DisplayName = DisplayName};
            return ret;
        }

        #endregion ICloneable Members
    }
}