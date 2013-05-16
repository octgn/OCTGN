using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Octgn.Data.Properties;
using System.IO;

namespace Octgn.Data
{
    using Octgn.Library;

    public class DatabaseHandler
    {
        private static Dictionary<string, List<string>> columnCache = new Dictionary<string, List<string>>();
        private static string suffix = "oldtable";

		public static string BasePath = SimpleConfig.DataDirectory;
		private static readonly string DatabasePath = Path.Combine(BasePath, "Database");
		private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db3");
		private static readonly string ConString = "URI=file:" + DatabaseFile;

		public static string GetUser(string jid)
		{
			using (var con = new SQLiteConnection(ConString))
			{
				con.Open();
				using(var com = con.CreateCommand())
				{
					com.CommandText = "SELECT email FROM users WHERE jid=@jid";
					com.CommandType = CommandType.Text;
					com.Parameters.AddWithValue("@jid" , jid);
					using(var r = com.ExecuteReader())
					{
						if(r.Read())
						{
							return r.GetString(0);
						}
					}
				}
				con.Close();
			}
			return null;
		}



        public static string GetXmlString(int id, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM xml WHERE id=@id AND gid=@gid";
                    com.CommandType = CommandType.Text;
                    com.Parameters.AddWithValue("@id", id);
                    com.Parameters.AddWithValue("@gid", gid);
                    using(var r = com.ExecuteReader())
					{
						if(r.Read())
						{
							return r.GetString(0);
						}
					}
				}
				con.Close();
            }
            return null;
        }

        public static void AddXmlString(string xml_link, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "INSERT INTO xml(xml_link, gid) VALUES(@xml_link, @gid)";
                    com.Parameters.AddWithValue("@xml_link", xml_link);
                    com.Parameters.AddWithValue("@gid", gid);
                    com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static List<String> GetAllXmls(string gid)
        {
            var result = new List<String>();
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "SELECT xml_link FROM xml WHERE gid=@gid AND xml_link != ''";
                    com.Parameters.AddWithValue("@gid", gid);
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
                con.Close();
            }
            return result;
        }

        public static void DeleteXml(string id, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "DELETE FROM xml WHERE gid=@gid AND id=@id";
                    com.Parameters.AddWithValue("@id", id);
                    com.Parameters.AddWithValue("@gid", gid);
                    com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static void DeleteXmlByLink(string xml_link, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "DELETE FROM xml WHERE gid=@gid AND xml_link=@xml_link";
                    com.Parameters.AddWithValue("@xml_link", xml_link);
                    com.Parameters.AddWithValue("@gid", gid);
                    com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static void AddOldXmlToXmlString(int id, string old_xml, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "UPDATE xml SET old_xml=@old_xml WHERE id=@id AND gid=@gid";
                    com.Parameters.AddWithValue("@old_xml", old_xml);
                    com.Parameters.AddWithValue("@gid", gid);
                    com.Parameters.AddWithValue("@id", id);
                    com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static String GetOldXmlByLink(string xml_link, string gid)
        {
            var result = "";
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "SELECT old_xml FROM xml WHERE gid=@gid AND xml_link=@xml_link";
                    com.Parameters.AddWithValue("@gid", gid);
                    com.Parameters.AddWithValue("@xml_link", xml_link);
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                result = reader.GetString(0);
                            }
                            catch
                            {
                                result = null;
                            }
                        }
                    }
                }
                con.Close();
            }
            return result;

        }

        public static void WriteOldXmlByLink(string xml_link, string old_xml, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "UPDATE xml SET old_xml=@old_xml WHERE xml_link=@xml_link AND gid=@gid";
                    com.Parameters.AddWithValue("@old_xml", old_xml);
                    com.Parameters.AddWithValue("@gid", gid);
                    com.Parameters.AddWithValue("@xml_link", xml_link);
                    com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static string SearchXmlByLink(string xml_link, string gid)
        {
            using (var con = new SQLiteConnection(ConString))
            {
                con.Open();
                using (var com = con.CreateCommand())
                {
                    com.CommandText = "SELECT xml_link FROM xml WHERE xml_link=@xml_link AND gid=@gid";
                    com.CommandType = CommandType.Text;
                    com.Parameters.AddWithValue("@xml_link", xml_link);
                    com.Parameters.AddWithValue("@gid", gid);
                    using (var r = com.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return r.GetString(0);
                        }
                    }
                }
                con.Close();
            }
            return null;

        }

		public static void AddUser(string jid, string email)
		{
			using (var con = new SQLiteConnection(ConString))
			{
				con.Open();
				using(var com = con.CreateCommand())
				{
					com.CommandText = "INSERT INTO users(jid,email) VALUES(@jid,@email)";
					com.CommandType = CommandType.Text;
					com.Parameters.AddWithValue("@jid" , jid);
					com.Parameters.AddWithValue("@email" , email);
					com.ExecuteNonQuery();
				}
			}
		}

        public static bool ColumnExists(string tableName, string columnName, SQLiteConnection connection)
        {
            if (!columnCache.ContainsKey(columnName) || !columnCache[tableName].Contains(columnName))
            {
                columnCache[tableName] = new List<string>();
                using (SQLiteCommand com = connection.CreateCommand())
                {
                    com.CommandText = "PRAGMA table_info(" + tableName + ")";
                    com.Prepare();
                    using (SQLiteDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int nameColumnOrdinal = reader.GetOrdinal("name");
                            string dbColumnName = reader.GetString(nameColumnOrdinal);
                            columnCache[tableName].Add(dbColumnName);
                        }
                    }
                }
            }
            return (columnCache[tableName].Contains(columnName));
        }

        public static bool AddColumn(string tableName, string columnName, PropertyType type, SQLiteConnection connection)
        {
            bool ret = false;

            using (SQLiteCommand com = connection.CreateCommand())
            {
                //YES I AM USING A STRINGBUILDER BECAUSE SQLITE WAS BEING A FUCKER. CHANGE IT IF YOU CAN MAKE IT WORK >_<
                //BLOODY PARAMETERS FUCKING UP CONSTANTLY. SQLITE IS SHIT IN MY OPINION </endrage>
                //TODO: Find out why ALTER commands do not always play nice with sqlitecommand parameters.
                StringBuilder sb = new StringBuilder("ALTER TABLE @tablename ADD [@columnname] @type DEFAULT '@default'");
                sb = sb.Replace("@tablename", tableName);
                sb = sb.Replace("@columnname", columnName);
                switch (type)
                {
                    case PropertyType.String:
                        sb = sb.Replace("@type", "TEXT");
                        sb = sb.Replace("@default", "");
                        break;
                    case PropertyType.Integer:
                        sb = sb.Replace("@type", "INTEGER");
                        sb = sb.Replace("@default", "");
                        break;
                    case PropertyType.GUID:
                        sb = sb.Replace("@type", "TEXT");
                        sb = sb.Replace("@default", "00000000-0000-0000-0000-000000000000");
                        break;
                    default:
                        sb = sb.Replace("@type", "TEXT");
                        sb = sb.Replace("@default", "");
                        break;
                }
                com.CommandText = sb.ToString();
                com.ExecuteNonQuery();
            }

            ret = ColumnExists(tableName, columnName, connection);
            return (ret);
        }

        public static void ClearDatabase(SQLiteConnection connection)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM [custom_properties]";
                command.ExecuteNonQuery();
                command.CommandText = "DELETE FROM [games]";
                command.ExecuteNonQuery();
            }
            RebuildCardTable(connection);
        }

        public static void RebuildCardTable(SQLiteConnection connection)
        {
            RemoveOldRemnants(connection);
            RenameTables(connection);
            MakeEmptyStructure(connection);
            CopyOverData(connection);
            CopyCardData(connection);
            DeleteTempTables(connection);
        }
        internal static void RemoveOldRemnants(SQLiteConnection connection)
        {
            if (ColumnExists(string.Format("{0}users", suffix), "email", connection))
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("DROP TABLE [{0}users];", suffix);
                    command.ExecuteNonQuery();
                }
            }
        }

        internal static void RenameTables(SQLiteConnection connection)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = string.Format("ALTER TABLE [dbinfo] RENAME TO [{0}dbinfo]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [games] RENAME TO [{0}games]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [sets] RENAME TO [{0}sets]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [cards] RENAME TO [{0}cards]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [custom_properties] RENAME TO [{0}custom_properties]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [markers] RENAME TO [{0}markers]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [packs] RENAME TO [{0}packs]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [users] RENAME TO [{0}users]", suffix);
                command.ExecuteNonQuery();
                command.CommandText = string.Format("ALTER TABLE [xml] RENAME TO [{0}xml]", suffix);
                command.ExecuteNonQuery();
            }

            using (SQLiteCommand command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = "DROP INDEX SearchIndex";
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    //this is just so it doesnt stop on a missing index.
                }
            }
        }

        internal static void MakeEmptyStructure(SQLiteConnection connection)
        {
            using (SQLiteCommand com = connection.CreateCommand())
            {
                string md = Resource1.MakeDatabase;
                md = md.Replace("begin transaction;", "");
                md = md.Replace("commit transaction;", "");
                com.CommandText = md;
                com.ExecuteNonQuery();
            }
            UpdateDatabase.Update(connection);
        }

        internal static void CopyOverData(SQLiteConnection connection)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("DELETE FROM [dbinfo]");
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                //dbinfo table
                sb.Clear();
                sb.Append(string.Format("INSERT INTO \"dbinfo\"(\"version\") SELECT \"version\" FROM \"{0}dbinfo\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                //games
                sb.Clear();
                sb.Append("INSERT INTO \"games\"(\"real_id\",\"id\",\"name\",\"filename\",\"version\",\"card_height\",\"card_width\",\"card_back\",\"deck_sections\",\"shared_deck_sections\",\"file_hash\") ");
                sb.Append(string.Format("SELECT \"real_id\",\"id\",\"name\",\"filename\",\"version\",\"card_height\",\"card_width\",\"card_back\",\"deck_sections\",\"shared_deck_sections\",\"file_hash\" FROM \"{0}games\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                //sets
                sb.Clear();
                sb.Append("INSERT INTO \"sets\"(\"real_id\", \"id\", \"name\", \"game_real_id\", \"game_version\", \"version\", \"package\") ");
                sb.Append(string.Format("SELECT \"real_id\", \"id\", \"name\", \"game_real_id\", \"game_version\", \"version\", \"package\" FROM \"{0}sets\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                //markers
                sb.Clear();
                sb.Append("INSERT INTO \"markers\"(\"real_id\",\"id\",\"game_id\",\"set_real_id\",\"name\",\"icon\") ");
                sb.Append(string.Format("SELECT \"real_id\",\"id\",\"game_id\",\"set_real_id\",\"name\",\"icon\" FROM \"{0}markers\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                
                //packs
                sb.Clear();
                sb.Append("INSERT INTO \"packs\"(\"real_id\",\"id\",\"set_real_id\",\"name\",\"xml\") ");
                sb.Append(string.Format("SELECT \"real_id\",\"id\",\"set_real_id\",\"name\",\"xml\" FROM \"{0}packs\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                //custom_properties
                sb.Clear();
                sb.Append("INSERT INTO \"custom_properties\"(\"real_id\",\"id\",\"card_real_id\",\"game_id\",\"name\",\"type\",\"vint\",\"vstr\") ");
                sb.Append(string.Format("SELECT \"real_id\",\"id\",\"card_real_id\",\"game_id\",\"name\",\"type\",\"vint\",\"vstr\" FROM \"{0}custom_properties\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                
                //users
                sb.Clear();
                sb.Append("INSERT INTO \"users\"(\"id\",\"jid\",\"email\") ");
                sb.Append(string.Format("SELECT \"id\", \"jid\", \"email\" FROM \"{0}users\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();


                //xml
                sb.Clear();
                sb.Append("INSERT INTO \"xml\"(\"id\",\"xml_link\",\"gid\",\"old_xml\") ");
                sb.Append(string.Format("SELECT \"id\",\"xml_link\",\"gid\",\"old_xml\" FROM \"{0}xml\"", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                
                //cards
            }
        }

        internal static void CopyCardData(SQLiteConnection connection)
        {
            List<Tuple<string, int>> fieldList = new List<Tuple<string, int>>();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT DISTINCT [name], [type], [game_id] FROM [custom_properties]";
                int nameOrdinal = -1;
                int typeOrdinal = -1;
                int guidOrdinal = -1;
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read() != false)
                    {
                        if (nameOrdinal < 0)
                        {
                            nameOrdinal = reader.GetOrdinal("name");
                            typeOrdinal = reader.GetOrdinal("type");
                            guidOrdinal = reader.GetOrdinal("game_id");
                        }
                        Tuple<string, int> nameTypePair = new Tuple<string, int>(reader.GetString(guidOrdinal).Replace("-","") + reader.GetString(nameOrdinal), reader.GetInt32(typeOrdinal));
                        fieldList.Add(nameTypePair);
                    }
                }
                columnCache.Clear();
                foreach (Tuple<string, int> nameTypePair in fieldList)
                {
                    if (!ColumnExists("cards", nameTypePair.Item1, connection))
                    {
                        switch (nameTypePair.Item2)
                        {
                            case 1:
                                AddColumn("cards", nameTypePair.Item1, PropertyType.Integer, connection);
                                break;
                            case 2:
                                AddColumn("cards", nameTypePair.Item1, PropertyType.GUID, connection);
                                break;
                            case 3:
                                AddColumn("cards", nameTypePair.Item1, PropertyType.Char, connection);
                                break;
                            default:
                                AddColumn("cards", nameTypePair.Item1, PropertyType.String, connection);
                                break;
                        }
                    }
                }
            }
            if (fieldList.Count > 0)
            {

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO \"cards\"(");
                    StringBuilder sb2 = new StringBuilder();
                    foreach (Tuple<string, int> nameTypePair in fieldList)
                    {
                        sb.Append(string.Format("\"{0}\",", nameTypePair.Item1));
                        sb2.Append(string.Format("\"{0}\",", nameTypePair.Item1));
                    }
                    foreach (string columnName in columnCache["cards"])
                    {
                        sb.Append(string.Format("\"{0}\",", columnName));
                        sb2.Append(string.Format("\"{0}\",", columnName));
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb2.Remove(sb2.Length - 1, 1);
                    sb.Append(string.Format(") SELECT {0} FROM \"{1}cards\"", sb2.ToString(), suffix));
                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();
                }
            }
        }

        internal static void DeleteTempTables(SQLiteConnection connection)
        {
            using (SQLiteCommand command = connection.CreateCommand())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("DROP TABLE [{0}dbinfo];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}games];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}sets];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}markers];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}packs];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}custom_properties];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}cards];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}users];", suffix));
                sb.Append(string.Format("DROP TABLE [{0}xml];", suffix));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
            }
        }
    }
}
