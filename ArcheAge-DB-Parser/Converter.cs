using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ArcheAge_DB_Parser
{
    class Converter
    {
        static BinaryWriter writer;

        static List<Table> tables;
        static List<string> lookupTable;

        static SQLiteConnection con;
        /*public static void convert(string db_filename, string cfg_filename)
        {
            try
            {
                writer = new BinaryWriter(File.Open(db_filename, FileMode.Open));

                string jsonData = File.ReadAllText(cfg_filename);
                tables = JsonConvert.DeserializeObject<List<Table>>(jsonData);
            }
            catch
            {
                Console.WriteLine("Failed to open database or config files.");
                Environment.Exit(0);
            }
            lookupTable.Clear();
            foreach (Table table in tables)
            {
                if (Parser.filterCustoms(table))
                    continue;

                while (readRow())
                {
                    int skips = 0;//Used for color optimization

                    updateConsole(db_filename);

                    if (!table.ignore)
                        sqlite_db.createQuery(table);

                    foreach (Column column in table.columns)
                    {
                        if (skips > 0)
                        {
                            skips--;
                            continue;
                        }
                        switch (column.type)
                        {
                            case "int":
                                int iData = readInt32();
                                sqlite_db.addToQuery(column.name, iData);
                                break;
                            case "double":
                                double dData = readDouble();
                                sqlite_db.addToQuery(column.name, dData);
                                break;
                            case "bool":
                                bool bData = readBool();
                                sqlite_db.addToQuery(column.name, bData);
                                break;
                            case "string":
                                string sData = readString();
                                sqlite_db.addToQuery(column.name, sData);
                                break;
                            case "time":
                                DateTime tData = readTime();
                                sqlite_db.addToQuery(column.name, tData);
                                break;
                            case "blob":
                                byte[] blob = readBlob();
                                sqlite_db.addToQuery(column.name, blob, blob.Length);
                                break;
                            case "color":
                                int data = reader.ReadInt32();
                                if (data != -1)
                                {
                                    //Ghetto Fix for Color Logic
                                    string newName = column.name.Substring(0, column.name.Length - 1);
                                    int r = reader.ReadInt32();
                                    sqlite_db.addToQuery(newName + "r", r);
                                    int g = reader.ReadInt32();
                                    sqlite_db.addToQuery(newName + "g", g);
                                    int b = reader.ReadInt32();
                                    sqlite_db.addToQuery(newName + "b", b);
                                }
                                skips = 2;
                                break;
                            default:
                                Console.Write("Unknown Data Type");
                                break;
                        }
                        //Console.Write(",");
                    }
                    if (!table.ignore)
                        sqlite_db.executeQuery();
                    //Console.WriteLine();
                }
                //Console.WriteLine("Successfully Parsed " + table.name);
            }
            sqlite_db.endTransaction();
            sqlite_db.closeDB();
        }
        */
    }
}
