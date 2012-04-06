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
                com.CommandText = "PRAGMA table_info(@tablename)";
                com.Parameters.AddWithValue("@tablename", tableName);
                SQLiteDataReader reader = com.ExecuteReader();
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
            return (ret);
        }

        public static bool AddColumn(string tableName, string columnName, PropertyType type, SQLiteConnection connection)
        {
            bool ret = false;

            using (SQLiteCommand com = connection.CreateCommand())
            {
                com.CommandText = "ALTER TABLE @tablename ADD @columnname @type DEFAULT '@default'";
                com.Parameters.AddWithValue("@tablename", tableName);
                com.Parameters.AddWithValue("@columnname", columnName);
                switch (type)
                {
                    case PropertyType.String:
                        com.Parameters.AddWithValue("@type", "TEXT");
                        com.Parameters.AddWithValue("@default", "");
                        break;
                    case PropertyType.Integer:
                        com.Parameters.AddWithValue("@type", "INTEGER");
                        com.Parameters.AddWithValue("@default", 0);
                        break;
                    case PropertyType.GUID:
                        com.Parameters.AddWithValue("@type", "TEXT");
                        com.Parameters.AddWithValue("@default", "00000000-0000-0000-0000-000000000000");
                        break;
                    default:
                        com.Parameters.AddWithValue("@type", "TEXT");
                        com.Parameters.AddWithValue("@default", "");
                        break;
                }
                com.ExecuteNonQuery();
            }

            ret = ColumnExists(tableName, columnName, connection);
            return (ret);
        }
    }
}
