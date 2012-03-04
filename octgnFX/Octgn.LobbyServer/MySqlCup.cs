using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using MySql.Data.MySqlClient;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public static class MySqlCup
    {
        public static readonly string ConnectionString = new MySqlConnectionStringBuilder()
                                                    {
                                                        Database = Program.Settings["db"],
                                                        UserID = Program.Settings["dbUser"],
                                                        Password = Program.Settings["dbPass"],
                                                        Server = Program.Settings["dbHost"]
                                                    }.ToString();

        private static ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        static MySqlCup()
        {

        }

        /// <summary>
        ///   Is the current user banned?
        /// </summary>
        /// <param name="uid"> User ID </param>
        /// <param name="endpoint"> The Endpoint </param>
        /// <returns> -1 if not banned. Timestamp of ban end if banned. Timestamp can be Converted to DateTime with fromPHPTime. </returns>
        public static int IsBanned(int uid, EndPoint endpoint)
        {
            
            if (uid <= -1)
                return -1;
            if (endpoint == null)
                return -1;
            int ret = -1;
            Locker.EnterUpgradeableReadLock();
            try
            {
                using (var Con = new MySqlConnection(ConnectionString))
                {
                    string ip = endpoint.ToString();

                    MySqlCommand cmd = Con.CreateCommand();
                    try
                    {
                        ip = ip.Substring(0, ip.IndexOf(':'));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(ip);
                    }

                    cmd.CommandText = "SELECT * FROM bans WHERE uid=@uid OR ip=@ip;";
                    cmd.Prepare();
                    cmd.Parameters.Add("@uid", MySqlDbType.Int32);
                    cmd.Parameters.Add("@ip", MySqlDbType.VarChar, 15);
                    cmd.Parameters["@uid"].Value = uid;
                    cmd.Parameters["@ip"].Value = ip;


                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            var bans = new List<Ban>();
                            while (dr.Read())
                            {
                                var b = new Ban
                                            {
                                                Bid = dr.GetInt32("bid"),
                                                Uid = dr.GetInt32("uid"),
                                                EndTime = dr.GetInt32("end"),
                                                Ip = dr.GetString("ip")
                                            };

                                bans.Add(b);
                            }
                            dr.Close();
                            foreach (Ban b in bans)
                            {
                                string bid = b.Bid.ToString(CultureInfo.InvariantCulture);
                                DateTime endtime = ValueConverters.FromPhpTime(b.EndTime);
                                if (DateTime.Now >= endtime)
                                {
                                    Locker.EnterWriteLock();
                                    DeleteRow(Con,"bans", "bid", bid);
                                    Locker.EnterReadLock();
                                }
                                else
                                {
                                    ret = (int) b.EndTime;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            dr.Close();

                        }
                    }
                }
            }
            catch (MySqlException me)
            {
                Logger.Er(me.InnerException);
            }
            Locker.ExitUpgradeableReadLock();
            return ret;
        }

        /// <summary>
        ///   Just a generic delete row function.
        /// </summary>
        /// <param name="Con"> Connection </param>
        /// <param name="table"> Table to delete from </param>
        /// <param name="columnname"> Column name to check against. </param>
        /// <param name="columnvalue"> The value, that if exists in said column, will cause the row to go bye bye. </param>
        private static void DeleteRow(MySqlConnection Con,string table, string columnname, string columnvalue)
        {
            MySqlCommand cmd = Con.CreateCommand();
            cmd.CommandText = "DELETE FROM @table WHERE @col=@val;";
            cmd.Prepare();
            cmd.Parameters.Add("@table");
            cmd.Parameters.Add("@col");
            cmd.Parameters.Add("@val");
            cmd.Parameters["@table"].Value = table;
            cmd.Parameters["@col"].Value = columnname;
            cmd.Parameters["@table"].Value = columnvalue;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///   Get user information from the database.
        /// </summary>
        /// <param name="email"> Users e-mail </param>
        /// <returns> User data, such as UID and whatnot, or NULL if none found. </returns>
        public static User GetUser(string email)
        {
            
            if (email == null)
                return null;
            if (String.IsNullOrWhiteSpace(email))
                return null;
            User ret = null;
            Locker.EnterReadLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM users WHERE email=@email;";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 60);
                    com.Parameters["@email"].Value = email;
                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ret = new User
                                        {
                                            Email = dr.GetString("email"),
                                            DisplayName = dr.GetString("name"),
                                            Uid = dr.GetInt32("uid"),
                                            CustomStatus = dr.GetString("status"),
                                            Status = UserStatus.Unknown,
                                            Level = (UserLevel) dr.GetInt32("level")
                                        };
                        }
                        dr.Close();
                    }
                    ;
                }
            }
            catch (Exception ex)
            {
                Logger.Er(ex);
            }
            Locker.ExitReadLock();
            return ret;
        }

        /// <summary>
        ///   Gets a user from the database based on there UID
        /// </summary>
        /// <param name="uid"> UID of the user </param>
        /// <returns> User that matches, or null. </returns>
        public static User GetUser(int uid)
        {
            if (uid <= -1)
                return null;
            User ret = null;
            Locker.EnterReadLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM users WHERE uid=@uid;";
                    com.Prepare();
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@uid"].Value = uid;
                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            ret = new User
                                        {
                                            Email = dr.GetString("email"),
                                            DisplayName = dr.GetString("name"),
                                            Uid = dr.GetInt32("uid"),
                                            CustomStatus = dr.GetString("status"),
                                            Status = UserStatus.Unknown,
                                            Level = (UserLevel) dr.GetInt32("level")
                                        };
                        }
                        dr.Close();
                    }
                    ;
                }
            }
            catch (Exception ex)
            {
                Logger.Er(ex);
            }
            Locker.ExitReadLock();
            return ret;
        }

        /// <summary>
        ///   Register a user to the database.
        /// </summary>
        /// <param name="email"> users e-mail </param>
        /// <param name="name"> display name </param>
        /// <returns> true on success, false on failure. </returns>
        public static bool RegisterUser(string email, string name)
        {
            if (email == null || name == null)
                return false;
            if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(name))
                return false;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "INSERT INTO users(email,name) VALUES(@email,@name);";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 60);
                    com.Parameters.Add("@name", MySqlDbType.VarChar, 60);
                    com.Parameters["@email"].Value = email;
                    com.Parameters["@name"].Value = name;
                    com.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Er(ex);
            }
            Locker.ExitWriteLock();
            return false;
        }

        /// <summary>
        ///   Remove a friend request from the database.
        /// </summary>
        /// <param name="requesteeuid"> Requestee's UID </param>
        /// <param name="friendemail"> To be friends email </param>
        public static void RemoveFriendRequest(int requesteeuid, string friendemail)
        {
            if (requesteeuid <= -1 || friendemail == null)
                return;
            if (String.IsNullOrWhiteSpace(friendemail))
                return;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand cmd = Con.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM friendrequests WHERE uid=@uid AND email=@email;";
                    cmd.Prepare();
                    cmd.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    cmd.Parameters.Add("@email", MySqlDbType.String, 100);
                    cmd.Parameters["@uid"].Value = requesteeuid;
                    cmd.Parameters["@email"].Value = friendemail;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
        }

        /// <summary>
        ///   Add a friend request. We use the friends email, because they might not have an account yet. So when they get an account, the system will realize it and send them a friend request.
        /// </summary>
        /// <param name="uid"> Users UID. </param>
        /// <param name="friendemail"> Friend-to-bes e-mail </param>
        public static void AddFriendRequest(int uid, string friendemail)
        {
            if (uid <= -1)
                return;
            if (friendemail == null)
                return;
            if (String.IsNullOrWhiteSpace(friendemail))
                return;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "INSERT INTO friendrequests(uid,email) VALUES(@uid,@email);";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 100);
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@email"].Value = friendemail;
                    com.Parameters["@uid"].Value = uid;
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
        }

        /// <summary>
        ///   Get a list of friend requests based on your e-mail address.
        /// </summary>
        /// <param name="email"> The users e-mail address </param>
        /// <returns> List of UID's of users that want to be the users friend. </returns>
        public static List<int> GetFriendRequests(string email)
        {
            if (email == null)
                return null;
            if (String.IsNullOrWhiteSpace(email))
                return null;
            List<int> ret = null;
            Locker.EnterReadLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM friendrequests WHERE email=@email;";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 100);
                    com.Parameters["@email"].Value = email;
                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (ret == null)
                                ret = new List<int>();
                            int uid = dr.GetInt32("uid");
                            ret.Add(uid);
                        }
                        dr.Close();
                    }
                    ;
                    return ret;
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitReadLock();
            return null;
        }

        /// <summary>
        ///   Add a friend. This happens after a successful friend request. It only needs to be called once, as it saves the friendship both ways. Even the order of userid, and friendid don't matter.
        /// </summary>
        /// <param name="useruid"> User id </param>
        /// <param name="frienduid"> Friend id </param>
        public static void AddFriend(int useruid, int frienduid)
        {
            if (useruid <= -1 || frienduid <= -1)
                return;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    List<User> myFriendList = GetFriendsList(useruid);
                    List<User> oFriendlist = GetFriendsList(frienduid);
                    com.CommandText = "INSERT INTO friends(uid,fid) VALUES(@uid,@fid);";
                    com.Prepare();
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters.Add("@fid", MySqlDbType.Int32, 11);
                    com.Parameters["@uid"].Value = useruid;
                    com.Parameters["@fid"].Value = frienduid;
                    if (!myFriendList.Exists(u => u.Uid == frienduid))
                        com.ExecuteNonQuery();
                    com.Parameters["@uid"].Value = frienduid;
                    com.Parameters["@fid"].Value = useruid;
                    if (!oFriendlist.Exists(u => u.Uid == useruid))
                        com.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
        }

        /// <summary>
        ///   Saves the users custom stats to the database.
        /// </summary>
        /// <param name="uid"> Users uid </param>
        /// <param name="status"> Custom status. </param>
        /// <returns> Returns a false if the data it got was bullshit. </returns>
        public static bool SetCustomStatus(int uid, string status)
        {
            if (uid <= -1)
                return false;
            if (status == null)
                return false;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "UPDATE users SET status=@status WHERE uid=@uid;";
                    com.Prepare();
                    com.Parameters.Add("@status", MySqlDbType.VarChar, 200);
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@status"].Value = status;
                    com.Parameters["@uid"].Value = uid;
                    com.ExecuteNonQuery();
                    Locker.ExitWriteLock();
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
            return false;
        }

        /// <summary>
        ///   Saves the users display name to the database
        /// </summary>
        /// <param name="uid"> Users uid </param>
        /// <param name="name"> Users display name </param>
        /// <returns> True on success, or false if the data is fucked. </returns>
        public static bool SetDisplayName(int uid, string name)
        {
            if (uid <= -1)
                return false;
            if (name == null)
                return false;
            Locker.EnterWriteLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "UPDATE users SET name=@name WHERE uid=@uid;";
                    com.Prepare();
                    com.Parameters.Add("@name", MySqlDbType.VarChar, 60);
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@name"].Value = name;
                    com.Parameters["@uid"].Value = uid;
                    com.ExecuteNonQuery();
                    Locker.ExitWriteLock();
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
            return false;
        }

        /// <summary>
        ///   Gets a list of friends based on your uid.
        /// </summary>
        /// <param name="uid"> Users UID </param>
        /// <returns> List of friends as Users, or NULL. </returns>
        public static List<User> GetFriendsList(int uid)
        {
            if (uid <= -1)
                return null;
            Locker.EnterReadLock();
            try
            {
                using(var Con = new MySqlConnection(ConnectionString))
                using (MySqlCommand com = Con.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM friends WHERE uid=@uid;";
                    com.Prepare();
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@uid"].Value = uid;
                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        var friends = new List<User>();
                        while (dr.Read())
                        {
                            User temp = GetUser(dr.GetInt32("fid"));
                            friends.Add(temp);
                        }
                        dr.Close();
                        Locker.ExitReadLock();
                        return friends;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Er(ex);
            }
            Locker.ExitReadLock();
            return null;
        }
    }
}