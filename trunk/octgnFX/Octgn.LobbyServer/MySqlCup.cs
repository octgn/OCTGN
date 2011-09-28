using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Skylabs.ConsoleHelper;
using Skylabs.Lobby;
using Skylabs.LobbyServer;

namespace Octgn.LobbyServer
{
    public class MySqlCup
    {
        public string DBUser { get; private set; }

        public string DBPass { get; private set; }

        public string DBHost { get; private set; }

        public string DBName { get; private set; }

        public string ConnectionString
        {
            get
            {
                MySql.Data.MySqlClient.MySqlConnectionStringBuilder sb = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
                sb.Database = DBName;
                sb.UserID = DBUser;
                sb.Password = DBPass;
                sb.Server = DBHost;
                return sb.ToString();
            }
        }

        public MySqlCup(string user, string pass, string host, string db)
        {
            DBUser = user;
            DBPass = pass;
            DBHost = host;
            DBName = db;
        }

        /// <summary>
        /// Is the current user banned?
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="c">client</param>
        /// <returns>-1 if not banned. Timestamp of ban end if banned. Timestamp can be converted to DateTime with fromPHPTime.</returns>
        public int IsBanned(int uid, Client c)
        {
            if(uid <= -1)
                return -1;
            int ret = -1;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    String ip = c.Sock.Client.RemoteEndPoint.ToString();
                    MySqlCommand cmd = con.CreateCommand();
                    ip = ip.Substring(0, ip.IndexOf(':'));

                    cmd.CommandText = "SELECT * FROM bans WHERE uid=@uid OR ip=@ip;";
                    cmd.Prepare();
                    cmd.Parameters.Add("@uid", MySqlDbType.Int32);
                    cmd.Parameters.Add("@ip", MySqlDbType.VarChar, 15);
                    cmd.Parameters["@uid"].Value = uid;
                    cmd.Parameters["@ip"].Value = ip;

                    using(MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if(dr.HasRows)
                        {
                            List<Ban> bans = new List<Ban>();
                            while(dr.Read())
                            {
                                Ban b = new Ban();

                                b.BID = dr.GetInt32("bid");
                                b.UID = dr.GetInt32("uid");
                                b.EndTime = dr.GetInt32("end");
                                b.IP = dr.GetString("ip");
                                bans.Add(b);
                            }
                            dr.Close();
                            foreach(Ban b in bans)
                            {
                                string bid = b.BID.ToString();
                                DateTime endtime = Skylabs.ValueConverters.fromPHPTime(b.EndTime);
                                if(DateTime.Now >= endtime)
                                {
                                    DeleteRow(con, "bans", "bid", bid);
                                }
                                else
                                {
                                    ret = (int)b.EndTime;
                                    break;
                                }
                            }
                            con.Close();
                        }
                        else
                        {
                            dr.Close();
                            con.Close();
                        }
                    }
                }
            }
            catch(MySqlException me)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError(me.Message, me), false);
#if(DEBUG)
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
            }
            return ret;
        }

        public void DeleteRow(MySqlConnection con, string table, string columnname, string columnvalue)
        {
            MySqlCommand cmd = con.CreateCommand();
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

        public User GetUser(string email)
        {
            if(email == null)
                return null;
            if(String.IsNullOrWhiteSpace(email))
                return null;
            User ret = null;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using(MySqlCommand com = con.CreateCommand())
                    {
                        com.CommandText = "SELECT * FROM users WHERE email=@email;";
                        com.Prepare();
                        com.Parameters.Add("@email", MySqlDbType.VarChar, 60);
                        com.Parameters["@email"].Value = email;
                        using(MySqlDataReader dr = com.ExecuteReader())
                        {
                            if(dr.Read())
                            {
                                ret = new User();
                                ret.Email = dr.GetString("email");
                                ret.DisplayName = dr.GetString("name");
                                ret.UID = dr.GetInt32("uid");
                                ret.Level = (Skylabs.Lobby.User.UserLevel)dr.GetInt32("level");
                            }
                            dr.Close();
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), false);
            }
            return ret;
        }

        public User GetUser(int uid)
        {
            if(uid <= -1)
                return null;
            User ret = null;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using(MySqlCommand com = con.CreateCommand())
                    {
                        com.CommandText = "SELECT * FROM users WHERE uid=@uid;";
                        com.Prepare();
                        com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                        com.Parameters["@uid"].Value = uid;
                        using(MySqlDataReader dr = com.ExecuteReader())
                        {
                            if(dr.Read())
                            {
                                ret = new User();
                                ret.Email = dr.GetString("email");
                                ret.DisplayName = dr.GetString("name");
                                ret.UID = dr.GetInt32("uid");
                                ret.Level = (Skylabs.Lobby.User.UserLevel)dr.GetInt32("level");
                            }
                            dr.Close();
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), false);
            }
            return ret;
        }

        public bool RegisterUser(string email, string name)
        {
            if(email == null || name == null)
                return false;
            if(String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(name))
                return false;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand com = con.CreateCommand();
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
            catch(Exception ex)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), false);
            }
            return false;
        }

        public void RemoveFriendRequest(int requesteeuid, string friendemail)
        {
            if(requesteeuid <= -1 || friendemail == null)
                return;
            if(String.IsNullOrWhiteSpace(friendemail))
                return;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand cmd = con.CreateCommand();
                    cmd.CommandText = "DELETE FROM friendrequests WHERE uid=@uid AND email=@email;";
                    cmd.Prepare();
                    cmd.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    cmd.Parameters.Add("@email", MySqlDbType.String, 100);
                    cmd.Parameters["@uid"].Value = requesteeuid;
                    cmd.Parameters["@email"].Value = friendemail;
                    cmd.ExecuteNonQuery();
                    return;
                }
            }
            catch(Exception e)
            {
            }
        }

        public void AddFriendRequest(int uid, string friendemail)
        {
            if(uid <= -1)
                return;
            if(friendemail == null)
                return;
            if(String.IsNullOrWhiteSpace(friendemail))
                return;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand com = con.CreateCommand();
                    com.CommandText = "INSERT INTO friendrequests(uid,email) VALUES(@uid,@email);";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 100);
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@email"].Value = friendemail;
                    com.Parameters["@uid"].Value = uid;
                    com.ExecuteNonQuery();
                    return;
                }
            }
            catch(Exception e)
            {
            }
        }

        public void AddFriend(int useruid, int frienduid)
        {
            if(useruid <= -1 || frienduid <= -1)
                return;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand com = con.CreateCommand();
                    com.CommandText = "INSERT INTO friends(uid,fid) VALUES(@uid,@fid);";
                    com.Prepare();
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters.Add("@fid", MySqlDbType.Int32, 11);
                    com.Parameters["@uid"].Value = useruid;
                    com.Parameters["@fid"].Value = frienduid;
                    com.ExecuteNonQuery();
                    com.Parameters["@uid"].Value = frienduid;
                    com.Parameters["@fid"].Value = useruid;
                    com.ExecuteNonQuery();
                    return;
                }
            }
            catch(Exception e)
            {
            }
        }

        public List<User> GetFriendsList(int uid)
        {
            if(uid <= -1)
                return null;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using(MySqlCommand com = con.CreateCommand())
                    {
                        com.CommandText = "SELECT * FROM friends WHERE uid=@uid;";
                        com.Prepare();
                        com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                        com.Parameters["@uid"].Value = uid;
                        using(MySqlDataReader dr = com.ExecuteReader())
                        {
                            List<User> friends = new List<User>();
                            while(dr.Read())
                            {
                                User temp = new User();
                                temp.Email = dr.GetString("email");
                                temp.Password = dr.GetString("password");
                                temp.DisplayName = dr.GetString("name");
                                temp.UID = dr.GetInt32("uid");
                                temp.Level = (Skylabs.Lobby.User.UserLevel)dr.GetInt32("level");
                                friends.Add(temp);
                            }
                            dr.Close();
                            con.Close();
                            return friends;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleEventLog.addEvent(new ConsoleEventError(ex.Message, ex), false);
            }
            return null;
        }
    }
}