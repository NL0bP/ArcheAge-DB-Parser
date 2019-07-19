using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace ArcheAge_DB_Parser
{
    class sqlite_db
    {
        static SQLiteConnection con;
        static SQLiteCommand cmd;
        public static void openDB(string db_filename)
        {
            con = new SQLiteConnection(String.Format("URI=file:{0}", db_filename));
            con.Open();
        }

        public static void closeDB()
        {
            con.Close();
        }

        public static void beginTransaction()
        {
            SQLiteCommand sqlComm = new SQLiteCommand("begin", con);
            sqlComm.ExecuteNonQuery();
        }

        public static void endTransaction()
        {
            SQLiteCommand sqlComm = new SQLiteCommand("end", con);
            sqlComm.ExecuteNonQuery();
        }

        public static void createTable(Table table)
        {
            string query = String.Format("CREATE TABLE {0} (", table.name);
            foreach (Column column in table.columns)
            {
                switch (column.type)
                {
                    case "int":
                        query += String.Format("{0} int {1},", column.name, 
                            (column.name=="id")?"PRIMARY KEY":"");
                        break;
                    case "double":
                        query += String.Format("{0} real,", column.name);
                        break;
                    case "bool":
                        query += String.Format("{0} bool,", column.name);
                        break;
                    case "string":
                        query += String.Format("{0} text,", column.name);
                        break;
                    case "color":
                        query += String.Format("{0} int,", column.name);
                        break;
                    case "time":
                        query += String.Format("{0} real,", column.name);
                        break;
                    case "blob":
                        query += String.Format("{0} blob,", column.name);
                        break;
                    default:
                        query += String.Format("{0} text,", column.name);
                        break;
                }
            }
            query = query.TrimEnd(',') + ");";
        }

        public static void createQuery(Table table)
        {
            if (String.IsNullOrEmpty(table.query))
            {
                string query = String.Format("INSERT INTO {0}(", table.name);
                foreach (Column column in table.columns)
                {
                    query += String.Format("{0},", column.name);
                }
                query = query.TrimEnd(',') + ") VALUES (";
                foreach (Column column in table.columns)
                {
                    query += String.Format("@{0},", column.name);
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
            string param = String.Format("@{0}", name);
            cmd.Parameters.Add(param, System.Data.DbType.Int32);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, bool value)
        {
            string param = String.Format("@{0}", name);
            cmd.Parameters.Add(param, System.Data.DbType.Boolean);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, string value)
        {
            string param = String.Format("@{0}", name);
            cmd.Parameters.Add(param, System.Data.DbType.String);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, DateTime value)
        {
            string param = String.Format("@{0}", name);
            long seconds = (long)(value.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            cmd.Parameters.Add(param, System.Data.DbType.Int64);
            cmd.Parameters[param].Value = seconds;

            // Maybe make this export as text date?
        }

        public static void addToQuery(string name, byte[] value, int len)
        {
            string param = String.Format("@{0}", name);
            cmd.Parameters.Add(param, System.Data.DbType.Binary,len);
            cmd.Parameters[param].Value = value;
        }

        public static void addToQuery(string name, double value)
        {
            string param = String.Format("@{0}", name);
            cmd.Parameters.Add(param, System.Data.DbType.Double);
            cmd.Parameters[param].Value = value;
        }
    }
}
