using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ArcheAge_DB_Parser
{
    class sqlite_db
    {
        static SQLiteConnection con;
        static SQLiteCommand cmd;

        static List<string> foreign_key_queries = new List<string>();

        public static void openDB(string db_filename)
        {
            con = new SQLiteConnection($"URI=file:{db_filename}");
            con.Open();
        }

        public static void closeDB()
        {
            con.Close();
        }

        public static void beginTransaction()
        {
            var sqlComm = new SQLiteCommand("begin", con);
            sqlComm.ExecuteNonQuery();
        }

        public static void endTransaction()
        {
            var sqlComm = new SQLiteCommand("end", con);
            sqlComm.ExecuteNonQuery();
        }

        static void checkForeignKey(string col_name, string tbl_name)
        {
            if (!col_name.EndsWith("_id"))
            {
                return;
            }

            var fName = col_name.Substring(0, col_name.IndexOf("_id"));
            foreach (var table in Parser.tables)
            {
                if (table.name == fName ||
                      table.name.TrimEnd('s') == fName ||
                      table.name == fName.TrimEnd('s'))
                {
                    foreign_key_queries.Add(
                        $",FOREIGN KEY({col_name}) REFERENCES {table.name}(id)"
                    );
                    return;
                }
            }
        }

        public static void createTable(Table table)
        {
            var query = $"CREATE TABLE IF NOT EXISTS  {table.name} (";
            foreach (var column in table.columns)
            {
                switch (column.type)
                {
                    case "int":
                        query += $"{column.name} int {(column.name == "id" ? "PRIMARY KEY" : "")},";
                        checkForeignKey(column.name, table.name);
                        break;
                    case "double":
                        query += $"{column.name} real,";
                        break;
                    case "bool":
                        query += $"{column.name} bool,";
                        break;
                    case "string":
                        query += $"{column.name} text,";
                        break;
                    case "color":
                        query += $"{column.name} int,";
                        break;
                    case "time":
                        query += $"{column.name} real,";
                        break;
                    case "blob":
                        query += $"{column.name} blob,";
                        break;
                    default:
                        query += $"{column.name} text,";
                        break;
                }
            }
            query = query.TrimEnd(',');
            foreach (var fKey in foreign_key_queries)
            {
                query += fKey;
            }
            foreign_key_queries.Clear();

            query += ");";
            var sqlComm = new SQLiteCommand(query, con);
            sqlComm.ExecuteNonQuery();
        }

        public static void UpdateRowInTable(Table table, Dictionary<string, object> columnValues, string condition)
        {
            string setValues = string.Join(", ", columnValues.Select(kv => $"{kv.Key} = @{kv.Key}"));
            string query = $"UPDATE {table.name} SET {setValues} WHERE {condition}";

            cmd = new SQLiteCommand(con);
            cmd.CommandText = query;

            foreach (var kv in columnValues)
            {
                cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
            }

            cmd.ExecuteNonQuery();
        }

        public static void AddColumnToTable(Table table, Column column)
        {
            string query = $"ALTER TABLE {table.name} ADD COLUMN {column.name} {column.type}";
            cmd = new SQLiteCommand(con);
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }

        public static void createQuery(Table table)
        {
            if (string.IsNullOrEmpty(table.query))
            {
                var query = $"INSERT INTO {table.name}(";
                foreach (var column in table.columns)
                {
                    query += $"{column.name},";
                }
                query = query.TrimEnd(',') + ") VALUES (";
                foreach (var column in table.columns)
                {
                    query += $"@{column.name},";
                }
                query = query.TrimEnd(',') + ");";
                table.query = query;
            }
            cmd = new SQLiteCommand(con);
            cmd.CommandText = table.query;
            cmd.Prepare();
        }

        public static void executeQuery()
        {
            cmd.ExecuteNonQuery();
        }

        public static void addToQuery(string name, int value)
        {
            var param = $"@{name}";
            cmd.Parameters.Add(param, System.Data.DbType.Int32);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, bool value)
        {
            var param = $"@{name}";
            cmd.Parameters.Add(param, System.Data.DbType.Boolean);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, string value)
        {
            var param = $"@{name}";
            cmd.Parameters.Add(param, System.Data.DbType.String);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, DateTime value)
        {
            var param = $"@{name}";
            var seconds = (long)value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            cmd.Parameters.Add(param, System.Data.DbType.Int64);
            cmd.Parameters[param].Value = seconds;

            // Maybe make this export as text date?
        }

        public static void addToQuery(string name, byte[] value, int len)
        {
            var param = $"@{name}";
            cmd.Parameters.Add(param, System.Data.DbType.Binary, len);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, double value)
        {
            var param = $"@{name}";
            cmd.Parameters.Add(param, System.Data.DbType.Double);
            cmd.Parameters[param].Value = value;
        }
    }
}
