using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Xml;
using Octgn.Data.Properties;
using System.Diagnostics;

namespace Octgn.Data
{
    class UpdateDatabase
    {

        public static bool Update(SQLiteConnection connection)
        {
            bool ret = false;
            int dbVersion = CurrentVersion(connection);
            while (GetQueries(dbVersion).Item1 != string.Empty)
            {
                using (SQLiteCommand com = connection.CreateCommand())
                {
                    Tuple<string, int> queryData = GetQueries(dbVersion);
                    com.CommandText = queryData.Item1;
                    int affectedRows = com.ExecuteNonQuery();

                    if (queryData.Item2 > 0)
                    {
                        com.CommandText = "UPDATE dbinfo SET version=" + queryData.Item2;
                        com.ExecuteNonQuery();
                    }
                    ret = ret ? true : (affectedRows > 0);
                }
                dbVersion = CurrentVersion(connection);
            }
            return (ret);
        }

        internal static int CurrentVersion(SQLiteConnection connection)
        {
            int ret = 0;
            using (SQLiteCommand com = connection.CreateCommand())
            {
                com.CommandText = "SELECT version FROM dbinfo";
                object scalarValue = com.ExecuteScalar();
                if (scalarValue != null)
                {
                    ret = int.Parse(scalarValue.ToString());
                }
            }
            return (ret);
        }

        internal static Tuple<string,int> GetQueries(int currentDBVersion)
        {
            Tuple<string, int> ret = new Tuple<string, int>(string.Empty, 0);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Resource1.UpdataDatabaseQueries);
            XmlNodeList nodes = doc.GetElementsByTagName("update");
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["from"] != null && int.Parse(node.Attributes["from"].Value) == currentDBVersion)
                {
                    string query = node.FirstChild.InnerText;
                    int newVersion = (node.Attributes["to"] != null) ? int.Parse(node.Attributes["to"].Value) : 0;
                    ret = new Tuple<string, int>(query, newVersion);
                }
            }

            return (ret);
        }
    }
}
