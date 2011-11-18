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
        public string DbUser { get; private set; }

        public string DbPass { get; private set; }

        public string DbHost { get; private set; }

        public string DbName { get; private set; }

        public string ConnectionString
        {
            get
            {
                MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder
                                                                             {
                                                                                 Database = DbName,
                                                                                 UserID = DbUser,
                                                                                 Password = DbPass,
                                                                                 Server = DbHost
                                                                             };
                return sb.ToString();
            }
        }

        public MySqlCup(string user, string pass, string host, string db)
        {
            DbUser = user;
            DbPass = pass;
            DbHost = host;
            DbName = db;
        }

        /// <summary>
        /// Is the current user banned?
        /// </summary>
        /// <param name="uid">User ID</param>
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
                                Ban b = new Ban
                                            {
                                                Bid = dr.GetInt32("bid"),
                                                Uid = dr.GetInt32("uid"),
                                                EndTime = dr.GetInt32("end"),
                                                Ip = dr.GetString("ip")
                                            };

                                bans.Add(b);
                            }
                            dr.Close();
                            foreach(Ban b in bans)
                            {
                                string bid = b.Bid.ToString();
                                DateTime endtime = Skylabs.ValueConverters.FromPhpTime(b.EndTime);
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
                ConsoleEventLog.AddEvent(new ConsoleEventError(me.Message, me), false);
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
                        con.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
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
                        con.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
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
                ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
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
                //TODO Should have some sort of logging here.
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
                //TODO Should have some sort of logging here.
            }
        }

        public List<int> GetFriendRequests(string email)
        {
            if (email == null)
                return null;
            if (String.IsNullOrWhiteSpace(email))
                return null;
            List<int >ret = null;
            try
            {
                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand com = con.CreateCommand();
                    com.CommandText = "SELECT * FROM friendrequests WHERE email=@email;";
                    com.Prepare();
                    com.Parameters.Add("@email", MySqlDbType.VarChar, 100);
                    com.Parameters["@email"].Value = email;
                    using (MySqlDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if(ret == null)
                                ret = new List<int>();
                            int uid = dr.GetInt32("uid");
                            ret.Add(uid);
                        }
                        dr.Close();
                    }
                    con.Close();
                    return ret;
                }
            }
            catch (Exception)
            {

            }
            return null;
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
                //TODO Should have some sort of logging here.
            }
        }

        public bool SetCustomStatus(int uid, string status)
        {
            if(uid <= -1)
                return false;
            if(status == null)
                return false;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    MySqlCommand com = con.CreateCommand();
                    com.CommandText = "UPDATE users SET status=@status WHERE uid=@uid;";
                    com.Prepare();
                    com.Parameters.Add("@status", MySqlDbType.VarChar, 200);
                    com.Parameters.Add("@uid", MySqlDbType.Int32, 11);
                    com.Parameters["@status"].Value = status;
                    com.Parameters["@uid"].Value = uid;
                    com.ExecuteNonQuery();
                    return true;
                }
            }
            catch(Exception e)
            {
                return false;
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
                                User temp = GetUser(dr.GetInt32("fid"));
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
                ConsoleEventLog.AddEvent(new ConsoleEventError(ex.Message, ex), false);
            }
            return null;
        }
    }
}