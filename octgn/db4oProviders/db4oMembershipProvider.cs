/// Copyright (c) 2008-2011 Brad Williams
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
/// associated documentation files (the "Software"), to deal in the Software without restriction,
/// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
/// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
/// subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial
/// portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
/// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
/// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Security;
using Octgn.Data;
using db4oProviders;
using WCSoft.Common;

namespace WCSoft.db4oProviders
{
    public class db4oMembershipProvider : MembershipProvider
    {
        private static readonly string PROVIDER_NAME = "db4oMembershipProvider";
        private readonly IConnectionStringStore ConnectionStringStore;
        private readonly int newPasswordLength = 5;
        private readonly IValidationKeyInfo ValidationKeyInfo;
        private string connectionString;

        public db4oMembershipProvider()
            : this(new ConfigurationManagerConnectionStringStore(), new ValidationKeyInfo("system.web/machineKey"))
        {
        }

        public db4oMembershipProvider(IConnectionStringStore connectionStringStore, IValidationKeyInfo validationKeyInfo)
        {
            ConnectionStringStore = connectionStringStore;
            ValidationKeyInfo = validationKeyInfo;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = PROVIDER_NAME;

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "db4o ASP.NET Membership Provider");
            }

            base.Initialize(name, config);

            applicationName = Utils.DefaultIfBlank(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

            connectionString = ConnectionStringStore.GetConnectionString(config["connectionStringName"]);
            if (connectionString == null)
                throw new ProviderException("Connection string cannot be blank.");

            maxInvalidPasswordAttempts = Convert.ToInt32(Utils.DefaultIfBlank(config["maxInvalidPasswordAttempts"], "5"));
            passwordAttemptWindow = Convert.ToInt32(Utils.DefaultIfBlank(config["passwordAttemptWindow"], "3"));
            minRequiredNonAlphanumericCharacters = Convert.ToInt32(Utils.DefaultIfBlank(config["minRequiredNonAlphanumericCharacters"], "0"));
            minRequiredPasswordLength = Convert.ToInt32(Utils.DefaultIfBlank(config["minRequiredPasswordLength"], "4"));
            passwordStrengthRegularExpression = Convert.ToString(Utils.DefaultIfBlank(config["passwordStrengthRegularExpression"], ""));
            enablePasswordReset = Convert.ToBoolean(Utils.DefaultIfBlank(config["enablePasswordReset"], "true"));
            enablePasswordRetrieval = Convert.ToBoolean(Utils.DefaultIfBlank(config["enablePasswordRetrieval"], "true"));
            requiresQuestionAndAnswer = Convert.ToBoolean(Utils.DefaultIfBlank(config["requiresQuestionAndAnswer"], "false"));
            requiresUniqueEmail = Convert.ToBoolean(Utils.DefaultIfBlank(config["requiresUniqueEmail"], "true"));

            string tempPasswordFormat = config["passwordFormat"] ?? "Hashed";

            switch (tempPasswordFormat)
            {
                case "Hashed":
                    passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            if (ValidationKeyInfo.IsAutoGenerated() && (PasswordFormat != MembershipPasswordFormat.Clear))
                throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!ValidateUser(username, oldPassword))
                return false;

            var args = new ValidatePasswordEventArgs(username, newPassword, false);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password cancelled due to new password validation failure.");

            User user = UpdateUser(username, found => found.Password = EncodePassword(newPassword));

            return (user != null);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username,
                                                             string password,
                                                             string newPwdQuestion,
                                                             string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            UpdateUser(username, updating =>
                                     {
                                         updating.PasswordQuestion = newPwdQuestion;
                                         updating.PasswordAnswer = EncodePassword(newPwdAnswer);
                                     });

            return true;
        }

        public override MembershipUser CreateUser(string username,
                                                  string password,
                                                  string email,
                                                  string passwordQuestion,
                                                  string passwordAnswer,
                                                  bool isApproved,
                                                  object providerUserKey,
                                                  out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                var user = new User(
                    (Guid) providerUserKey,
                    username,
                    EncodePassword(password),
                    email,
                    passwordQuestion,
                    EncodePassword(passwordAnswer),
                    isApproved,
                    "",
                    createDate,
                    createDate,
                    createDate,
                    applicationName,
                    false,
                    createDate,
                    0,
                    createDate,
                    0,
                    createDate);

                using (var dbc = Database.GetClient())
                {
                    dbc.Store(user);
                }

                status = MembershipCreateStatus.Success;

                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var dbc = Database.GetClient())
            {
                var results = dbc.Query<User>(u => u.Username == username && u.ApplicationName == applicationName);

                if (results.Count != 1)
                    return false;

                User found = results[0];

                dbc.Delete(found);
            }

            return false;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsers(
                u => u.ApplicationName == applicationName,
                pageIndex,
                pageSize,
                out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            List<User> users = GetUsers(u => u.LastActivityDate > compareTime && u.ApplicationName == applicationName);

            return users.Count;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
                throw new ProviderException("Password Retrieval not enabled.");

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            User found = GetUser(u => u.Username == username && u.ApplicationName == applicationName, false);

            if (found == null)
                throw new MembershipPasswordException("The supplied user name is not found.");

            if (found.IsLockedOut)
                throw new MembershipPasswordException("The supplied user is locked out.");

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, found.PasswordAnswer))
            {
                UpdateUser(username, updating => updating.UpdateFailureCount("passwordAnswer", this));

                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
                return UnEncodePassword(found.Password);

            return found.Password;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            User found = GetUser(user => user.Username == username && user.ApplicationName == applicationName, userIsOnline);
            if (found != null)
                return UserToMembershipUser(found);
            else
                return null;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            User found = GetUser(user => user.PKID == (Guid) providerUserKey && user.ApplicationName == applicationName, userIsOnline);
            if (found != null)
                return UserToMembershipUser(found);
            else
                return null;
        }

        private User GetUser(Predicate<User> userPredicate, bool userIsOnline)
        {
            using (var dbc = Database.GetClient())
            {
                var results = dbc.Query(userPredicate);

                if (results.Count == 0)
                    return null;

                User found = results[0];

                if (userIsOnline)
                {
                    found.LastActivityDate = DateTime.Now;
                    dbc.Store(found);
                }

                return found;
            }
        }

        private List<User> GetUsers(Predicate<User> userPredicate)
        {
            using (var dbc = Database.GetClient())
                return new List<User>(dbc.Query(userPredicate));
        }

        private MembershipUser UserToMembershipUser(User user)
        {
            return new MembershipUser(Name,
                                      user.Username,
                                      user.PKID,
                                      user.Email,
                                      user.PasswordQuestion,
                                      user.Comment,
                                      user.IsApproved,
                                      user.IsLockedOut,
                                      user.CreationDate,
                                      user.LastLoginDate,
                                      user.LastActivityDate,
                                      user.LastPasswordChangedDate,
                                      user.LastLockedOutDate);
        }

        public override bool UnlockUser(string username)
        {
            User found = UpdateUser(username,
                                    updating =>
                                        {
                                            updating.IsLockedOut = false;
                                            updating.LastLockedOutDate = DateTime.Now;
                                        });

            return found != null;
        }

        public override string GetUserNameByEmail(string email)
        {
            User found = GetUser(user => (user.Email == email && user.ApplicationName == ApplicationName), false);

            if (found == null)
                return "";
            else
                return found.Username;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
                throw new NotSupportedException("Password reset is not enabled.");

            User user = GetUser(u => u.Username == username && u.ApplicationName == applicationName, false);
            if (user == null)
                throw new ProviderException("The supplied user name is not found.");

            if (String.IsNullOrEmpty(answer) && RequiresQuestionAndAnswer)
            {
                user.UpdateFailureCount("passwordAnswer", this);
                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword = GenerateTempPassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            var args = new ValidatePasswordEventArgs(username, newPassword, false);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;

                throw new MembershipPasswordException("Reset password cancelled due to password validation failure.");
            }

            if (user.IsLockedOut)
                throw new MembershipPasswordException("The supplied user is locked out.");

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
            {
                user.UpdateFailureCount("passwordAnswer", this);
                throw new MembershipPasswordException("Incorrect password answer");
            }

            UpdateUser(user.Username,
                       updating =>
                           {
                               updating.Password = EncodePassword(newPassword);
                               updating.LastPasswordChangedDate = DateTime.Now;
                           });

            return newPassword;
        }

        /// <summary>
        /// Membership.GeneratePassword can generates some unnecessary complex password,
        /// whereas this produces passwords much more pleasant especially if no alphanumerics are required.
        /// </summary>
        protected static string GenerateTempPassword(int length, int minNonAlphanumeric)
        {
            int modVal = 3;

            if (minNonAlphanumeric == 0)
                modVal = 2;
            else if (minNonAlphanumeric * 3 > length)
                length = minNonAlphanumeric * 3;

            byte[] buffer = new byte[length];
            new Random((int) DateTime.Now.Ticks).NextBytes(buffer);

            string password = "";

            for (int i = 0; i < length; i++)
            {
                if (i % modVal == 0)
                    password = password + (char)('a' + (buffer[i] % 26));
                else if (i % modVal == 1)
                    password = password + (char)('0' + (buffer[i] % 10));
                else
                    password = password + (char)('#' + (buffer[i] % 4));
            }

            return password;
        }

        public override void UpdateUser(MembershipUser user)
        {
            UpdateUser(u => u.Username == user.UserName && u.ApplicationName == ApplicationName,
                       updating =>
                           {
                               updating.Email = user.Email;
                               updating.Comment = user.Comment;
                               updating.IsApproved = user.IsApproved;
                           });
        }

        public User UpdateUser(string username, Action<User> userUpdate)
        {
            return UpdateUser(u => u.Username == username && u.ApplicationName == ApplicationName, userUpdate);
        }

        private User UpdateUser(Predicate<User> userPredicate, Action<User> userUpdate)
        {
            using (var dbc = Database.GetClient())
            {
                IList<User> results = dbc.Query(userPredicate);

                if (results.Count != 1)
                    return null;

                User found = results[0];

                if (userUpdate != null)
                    userUpdate(found);

                found.LastActivityDate = DateTime.Now;

                dbc.Store(found);

                return found;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            User user = UpdateUser(username,
                                   updating =>
                                       {
                                           if (CheckPassword(password, updating.Password))
                                           {
                                               if (updating.IsApproved)
                                               {
                                                   isValid = true;
                                                   updating.LastLoginDate = DateTime.Now;
                                               }
                                           }
                                           else
                                           {
                                               updating.UpdateFailureCount("password", this);
                                           }
                                       }
                );

            if (user == null)
                return false;

            return isValid;
        }

        private bool CheckPassword(string password, string dbPassword)
        {
            string pass1 = password;
            string pass2 = dbPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbPassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
                return true;

            return false;
        }

        private string EncodePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "";

            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1 {Key = HexToByte(ValidationKeyInfo.GetKey())};
                    encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            if (string.IsNullOrEmpty(encodedPassword))
                return "";

            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                        Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length/2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i*2, 2), 16);
            return returnBytes;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsers(
                u => u.Username.Contains(usernameToMatch) && u.ApplicationName == applicationName,
                pageIndex,
                pageSize,
                out totalRecords);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsers(
                u => u.Email.Contains(emailToMatch) && u.ApplicationName == applicationName,
                pageIndex,
                pageSize,
                out totalRecords);
        }

        private MembershipUserCollection FindUsers(Predicate<User> predicate, int pageIndex, int pageSize, out int totalRecords)
        {
            List<User> list = GetUsers(predicate);
            totalRecords = list.Count;

            list.Sort(
                (left, right) => left.Username.CompareTo(right.Username));

            MembershipUserCollection result = new MembershipUserCollection();

            int start = pageIndex*pageSize;
            int end = (start + pageSize < totalRecords) ? start + pageSize : totalRecords;

            if (start < end)
            {
                for (int i = start; i < end; i++)
                    result.Add(UserToMembershipUser(list[i]));
            }

            return result;
        }

        #region properties

        private string applicationName;
        private bool enablePasswordReset;
        private bool enablePasswordRetrieval;
        private int maxInvalidPasswordAttempts;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private int passwordAttemptWindow;
        private MembershipPasswordFormat passwordFormat;
        private string passwordStrengthRegularExpression;
        private bool requiresQuestionAndAnswer;
        private bool requiresUniqueEmail;

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        #endregion
    }
}