using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Skylabs.Lobby
{
    [Serializable]
    public enum UserLevel { Standard = 0, Moderator = 1, Admin = 2 };

    [Serializable]
    public enum UserStatus { Unknown = -1, Offline = 0, Online = 1, Away = 2, DoNotDisturb = 3, Invisible = 4 };

    [Serializable]
    public class User : IEquatable<User>, ICloneable
    {
        public int UID { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }

        public string CustomStatus { get; set; }

        public UserStatus Status { get; set; }

        public UserLevel Level { get; set; }

        public User()
        {
        }

        public bool Equals(User u)
        {
            return (UID == u.UID);
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

        #region ICloneable Members

        public object Clone()
        {
            User ret = new User();
            ret.Email = Email;
            ret.UID = UID;
            ret.Password = Password;
            ret.Level = Level;
            ret.DisplayName = DisplayName;
            return ret;
        }

        #endregion ICloneable Members
    }
}