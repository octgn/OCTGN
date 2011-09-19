using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
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
        public int IsBanned(User user, Client c)
        {
            if(user == null)
                throw new NullReferenceException("User can't be null.");
            if(user.UID <= -1)
                throw new FormatException("User.UID not valid.");
            int ret = -1;
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    String ip = c.Sock.Client.RemoteEndPoint.ToString();
                    ip = ip.Substring(0, ip.IndexOf(':'));
                    using(MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM bans WHERE uid='" + user.UID.ToString() + "' OR ip='" + ip + "';", con))
                    {
                        using(MySqlDataReader dr = da.SelectCommand.ExecuteReader())
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
            }
            catch(MySqlException me)
            {
#if(DEBUG)
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
            }
            return ret;
        }

        public void DeleteRow(MySqlConnection con, string table, string columnname, string columnvalue)
        {
            string deleteSQL = "DELETE FROM " + table + " WHERE " + columnname + "='" + columnvalue + "';";
            MySqlCommand cmd2 = new MySqlCommand(deleteSQL, con);
            cmd2.ExecuteNonQuery();
        }

        public User GetUser(string email)
        {
            if(email == null)
                throw new NullReferenceException("Email can't be null.");
            try
            {
                using(MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    con.Open();
                    using(MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM users WHERE email='" + email + "';", con))
                    {
                        using(MySqlDataReader dr = da.SelectCommand.ExecuteReader())
                        {
                            if(dr.HasRows)
                            {
                                dr.Read();
                                User u = new User();
                                u.Email = dr.GetString("email");
                                u.Password = dr.GetString("password");
                                u.DisplayName = dr.GetString("name");
                                u.UID = dr.GetInt32("uid");
                                dr.Close();
                                con.Close();
                                return u;
                            }
                            else
                            {
                                dr.Close();
                                con.Close();
                                return null;
                            }
                        }
                    }
                }
            }
            catch(MySqlException ex)
            {
#if(DEBUG)
                if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
            }
            return null;
        }
    }
}