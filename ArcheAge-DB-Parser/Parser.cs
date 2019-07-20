using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        const int START_OF_ROW = 100;
        const int END_OF_TABLE = 101;

        public static List<Table> tables;
        static BinaryReader reader;

        static List<string> lookupTable = new List<string>();

        static List<string> stringTest = new List<string>();
        static List<int> idLog = new List<int>();

        static int counter;
        public static void updateConsole(string db_filename)
        {
            counter++;
            if (counter > 5000)
            {
                Console.Clear();
                Console.WriteLine("Parsing [{0}] {1:P}", db_filename,
                    (double)reader.BaseStream.Position / (double)reader.BaseStream.Length);
                counter = 0;
            }


        }

        public static bool filterCustoms(Table table)
        {
            if (table.name == "wearable_slots" && table.columns.Count == 2)
            {
                wearable_slots.parse(table);
                return true;
            }
            if (table.name == "allowed_name_chars")
            {
                allowed_name_chars.parse(table);
                return true;
            }
            if (table.name == "item_configs")
            {
                item_configs.parse(table);
                return true;
            }
            if (table.name == "wearables")
            {
                wearables.parse(table);
                return true;
            }
            return false;
        }

        public static void parse(string db_filename, string cfg_filename)
        {
            try
            {
                reader = new BinaryReader(File.Open(db_filename, FileMode.Open));

                string jsonData = File.ReadAllText(cfg_filename);
                tables = JsonConvert.DeserializeObject<List<Table>>(jsonData);
            }
            catch
            {
                Console.WriteLine("Failed to open database or config files.");
                Environment.Exit(0);
            }
            lookupTable.Clear();
            sqlite_db.openDB("export.db");
            sqlite_db.beginTransaction();
            foreach (Table table in tables)
            {
                int rowCount;
                if (filterCustoms(table))
                    continue;
                if (table.hasCount && readRow())
                    rowCount = reader.ReadInt32();

                if (!table.ignore)
                {
                    sqlite_db.createTable(table);
                }

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
                                    string newName = column.name.Substring(0,column.name.Length - 1);
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
                Console.WriteLine("Successfully Parsed " + table.name);
            }
            sqlite_db.endTransaction();
            sqlite_db.closeDB();
        }

        static bool readRow()
        {
            int data = reader.ReadByte();
            switch (data)
            {
                case START_OF_ROW:
                    return true;
                case END_OF_TABLE:
                    return false;
                default:
                    Console.WriteLine(String.Format("Parsing Error Occured on readRow() at offer [{0:x}].",
                        reader.BaseStream.Position
                    ));
                    Environment.Exit(0);
                    return false;
            }
        }

        static string readCString()
        {
            string data = "";
            char nextChar;

            while ((nextChar = reader.ReadChar()) != '\0')
            {
                data += nextChar;
            }
            return data;
        }
        static int readInt32()
        {
            return reader.ReadInt32();
        }

        static double readDouble()
        {
            return reader.ReadDouble();
        }

        static bool readBool()
        {
            return reader.ReadBoolean();
        }

        static string readString()
        {
            string data;

            byte strType = reader.ReadByte();
            if (strType == 0x2)
            {
                return "";
            }
            else if (strType == 0x1)
            {
                Int32 LTOffset = reader.ReadInt32();

                if (LTOffset == -1)
                {
                    data = readCString();
                    lookupTable.Add(data);
                    return data;
                }
                else
                {
                    return lookupTable[LTOffset];
                }
            }
            else if (strType == 0x0)
            {
                return readCString();
            }
            else
            {
                Console.WriteLine(
                    String.Format("Error Parsing String. Unrecognized Type({0}) at offset [{1:x}]",
                    strType,reader.BaseStream.Position
                 ));
                Environment.Exit(0);
                return "";
            }
        }

        static int readColor()
        {
            int data = reader.ReadInt32();
            if (data == -1)
                return -1;
            else
            {
                int r = reader.ReadInt32();
                int g = reader.ReadInt32();
                int b = reader.ReadInt32();
                return r + g + b;
            }
        }

        static DateTime readTime()
        {
            long data = reader.ReadInt64();
            DateTime date = new DateTime(1970, 1, 1).AddSeconds(data);
            return date;
        }

        static byte[] readBlob()
        {
            int size = reader.ReadInt32();
            byte[] data = reader.ReadBytes(size);

            return data;
        }
    }
}
