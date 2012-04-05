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
            while (GetQueries(dbVersion) != string.Empty)
            {
                using (SQLiteCommand com = connection.CreateCommand())
                {
                    com.CommandText = GetQueries(dbVersion);
                    int affectedRows = com.ExecuteNonQuery();

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
                int value = 0;
                if (scalarValue != null)
                {
                    ret = int.Parse(scalarValue.ToString());
                }
            }
            return (ret);
        }

        internal static string GetQueries(int currentDBVersion)
        {
            string ret = string.Empty;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Resource1.UpdataDatabaseQueries);
            XmlNodeList nodes = doc.GetElementsByTagName("update");
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["from"] != null && int.Parse(node.Attributes["from"].Value) == currentDBVersion)
                {
                    ret = node.FirstChild.InnerText;
                }
            }

            return (ret);
        }
    }
}
