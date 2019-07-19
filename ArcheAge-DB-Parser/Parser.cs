using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace ArcheAge_DB_Parser
{
    class Column
    {
        public string name;
        public string type;
    }
    class Table
    {
        public string name;
        public List<Column> columns;
        public int offset;
        public bool hasCount;
    }
    partial class Parser
    {
        const int START_OF_ROW = 100;
        const int END_OF_TABLE = 101;

        static List<Table> tables;
        static BinaryReader reader;

        static List<string> lookupTable = new List<string>();

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

            foreach (Table table in tables)
            {
                int rowCount;
                if (table.name == "wearable_slots" && table.columns.Count == 2)
                {
                    wearable_slots.parse();
                    continue;
                }
                if (table.name == "allowed_name_chars")
                {
                    allowed_name_chars.parse();
                    continue;
                }
                if (table.name == "item_configs")
                {
                    item_configs.parse();
                    continue;
                }
                if (table.hasCount && readRow())
                    rowCount = reader.ReadInt32();
                while (readRow())
                {
                    int skips = 0;//Used for color optimization
                    foreach (Column column in table.columns)
                    {
                        if (skips > 0)
                        {
                            skips--;
                            continue;
                        }
                        switch(column.type)
                        { 
                            case "int":
                                int iData = readInt32();
                                //Console.Write(iData);
                                break;
                            case "double":
                                double dData = readDouble();
                                //Console.Write(dData);
                                break;
                            case "bool":
                                bool bData = readBool();
                                //Console.Write(readBool());
                                break;
                            case "string":
                                string sData = readString();
                                //Console.Write(sData);
                                break;
                            case "color":
                                int cData = readColor();
                                skips = 2;
                                break;
                            case "time":
                                double tData = readDouble();
                                //.Write(tData);
                                break;
                            case "blob":
                                byte[] blob = readBlob();
                                //Console.Write(blob);
                                break;
                            default:
                                Console.Write("Unknown Data Type");
                                break;
                        }
                        //Console.Write(",");
                    }
                    //Console.WriteLine();
                }
                Console.WriteLine("Successfully Parsed " + table.name);
            }
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
                int g = reader.ReadInt32() >> 8;
                int b = reader.ReadInt32() >> 16;
                return r + g + b;
            }
        }

        static double readTime()
        {
            double data = reader.ReadDouble();
            return data;
        }

        static byte[] readBlob()
        {
            int size = reader.ReadInt32();

            return reader.ReadBytes(size);
        }
    }
}
