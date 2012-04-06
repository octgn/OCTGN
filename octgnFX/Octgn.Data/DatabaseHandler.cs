using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Octgn.Data
{
    class DatabaseHandler
    {
        public static bool ColumnExists(string tableName, string columnName, SQLiteConnection connection)
        {
            bool ret = false;
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
                        if (columnName.Equals(dbColumnName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            return (ret);
        }

        public static bool AddColumn(string tableName, string columnName, PropertyType type, SQLiteConnection connection)
        {
            bool ret = false;

            using (SQLiteCommand com = connection.CreateCommand())
            {
                //YES I AM USING A STRINGBUILDER BECAUSE SQLITE WAS BEING A FUCKER. CHANGE IT IF YOU CAN MAKE IT WORK >_<
                //BLOODY PARAMETERS FUCKING UP CONSTANTLY. SQLITE IS SHIT IN MY OPINION </endrage>
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

        public static string SanatizeColumnNames(string columnName)
        {
            return (columnName.Replace(" ", "_"));
        }

        public static string UnSanatizeColumnNames(string columnName)
        {
            return (columnName.Replace("_", " "));
        }
    }
}
