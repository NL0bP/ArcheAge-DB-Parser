using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        public static Parser instance { get; } = new();
        const int START_OF_ROW = 100;
        const int END_OF_TABLE = 101;

        public static Table localize;
        public static List<Table> tables;
        static BinaryReader reader;

        static string writePath = @"logging.txt";

        static List<string> lookup_table = new List<string>();

        static int counter;
        private Parser()
        {
            localize = new Table();
        }
        public static void updateConsole(string db_filename)
        {
            counter++;
            if (counter > 5000)
            {
                Console.Clear();
                using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                {
                    sw.WriteLine("Parsing [{0}] {1:P}", db_filename, reader.BaseStream.Position / (double)reader.BaseStream.Length);
                }
                Console.WriteLine("Parsing [{0}] {1:P}", db_filename, reader.BaseStream.Position / (double)reader.BaseStream.Length);
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

        public static void parse(string db_filename, string cfg_filename, int offset = 0)
        {
            try
            {
                // Убедитесь, что файл существует и удалите его, если нужно
                if (File.Exists(writePath))
                {
                    File.Delete(writePath);
                }

                reader = new BinaryReader(File.Open(db_filename, FileMode.Open));
                if (offset > 0)
                {
                    // Смещаем позицию чтения на offset байт
                    //reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var sk = reader.ReadBytes(offset);
                }

                var jsonData = File.ReadAllText(cfg_filename);
                tables = JsonConvert.DeserializeObject<List<Table>>(jsonData); // здесь можно увидеть строку с ошибкой в .json
            }
            catch
            {
                using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                {
                    sw.WriteLine("Failed to open database or config files.");
                }
                Console.WriteLine("Failed to open database or config files.");
                Environment.Exit(0);
            }
            lookup_table.Clear();
            sqlite_db.openDB("export.db");
            sqlite_db.beginTransaction();
            foreach (var table in tables)
            { 
                int rowCount = -1;
                var columnLangValues = new Dictionary<string, object>();
                var nameLang = "en_us";
                var indx = 1u;

                if (filterCustoms(table))
                {
                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("filterCustoms: Table " + table.name);
                    continue;
                }

                if (table.skipServerTable)
                {
                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("table.skipServerTable=true: Table " + table.name + ", let's skip the table");
                    continue;
                }

                if (table.hasCount && readRow())
                {
                    rowCount = reader.ReadInt32();

                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("table.hasCount=" + rowCount + ": Reading the number of records...");
                }
                else
                {
                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("table.hasCount=false: Skip reading the number of records...");
                }

                if (!table.ignore)
                {
                    if (table.name == "localized_texts")
                    {
                        if (localize.columns == null)
                        {
                            localize = table;
                            sqlite_db.createTable(table);
                        }
                        else
                        {
                            sqlite_db.AddColumnToTable(table, table.columns[table.columns.Count - 1]);
                            nameLang = table.columns[table.columns.Count - 1].name;
                        }
                    }
                    else
                    {
                        sqlite_db.createTable(table);
                    }

                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("Table: {0}", table.name + " Let's try to parse the table...");
                    foreach (var clmn in table.columns)
                    {
                        sw.WriteLine("Field: {0}", clmn.name);
                    }
                }

                var skipped = 0; // Used for localized

                while (readRow())
                {
                    var skips = 0; // Used for color optimization

                    updateConsole(db_filename);

                    if (!table.ignore)
                    {
                        sqlite_db.createQuery(table);
                    }
                    foreach (var column in table.columns)
                    {
                        if (table.ignore)
                        {
                            using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                            sw.WriteLine("table.ignore=true: Skip data recording...");
                        }

                        if (skips > 0)
                        {
                            skips--;
                            continue;
                        }
                        switch (column.type)
                        {
                            case "real": // hardfix `map_icons` table
                                var rData = readReal();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, rData);
                                }
                                break;
                            case "int2": // hardfix `map_icons` table
                                var iData2 = readInt2();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, iData2);
                                }

                                break;
                            case "int":
                                var iData = readInt32();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, iData);
                                }

                                break;
                            case "double":
                                var dData = readDouble();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, dData);
                                }

                                break;
                            case "bool":
                                var bData = readBool();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, bData);
                                }

                                break;
                            case "string":
                                var sData = readString();

                                //// расскоменируй это: for localize_text - в базу добавлен дополнительный столбец ID. Работает когда есть готовая база с другим языком, например английским и мы добавляем другой, например русский
                                //if (localize.columns != null)
                                //{
                                //    //    // этот свитч для попытки добавить в локализацию 3.0.4.2 русский язык из версии 3.0.3.0
                                //    //    switch (indx)
                                //    //    {
                                //    //        case 154713:
                                //    //            indx = 154715;
                                //    //            break;
                                //    //        case 155265:
                                //    //            indx = 155267;
                                //    //            break;
                                //    //        case 155318:
                                //    //            indx = 155320;
                                //    //            break;
                                //    //        case 155321:
                                //    //            indx = 155322;
                                //    //            break;
                                //    //        case 155337:
                                //    //            indx = 155338;
                                //    //            break;
                                //    //        case 313986:
                                //    //            skipped = 1; // сдвиг на 1 строку выше
                                //    //            break;
                                //    //        case 314029:
                                //    //            skipped = 0; // отмена сдвига строк выше
                                //    //            break;
                                //    //        case 327022:
                                //    //            skipped = 2; // сдвиг строк выше
                                //    //            break;
                                //    //        case 372629:
                                //    //            skipped = 0; // отмена сдвига строк выше
                                //    //            indx = 372631;
                                //    //            break;
                                //    //        case 404972:
                                //    //            indx = 404974;
                                //    //            break;
                                //    //        case 406524:
                                //    //            indx = 406527;
                                //    //            break;
                                //    //        case 406603:
                                //    //            indx = 406604;
                                //    //            break;
                                //    //        case 430989:
                                //    //            indx = 430991;
                                //    //            break;
                                //    //        case 431189:
                                //    //            indx = 431192;
                                //    //            break;
                                //    //        case 443488:
                                //    //            indx = 443490;
                                //    //            break;
                                //    //        case 447556:
                                //    //            indx = 447558;
                                //    //            break;
                                //    //        case 477262:
                                //    //            indx = 477265;
                                //    //            break;
                                //    //        case 482063:
                                //    //            indx = 482067;
                                //    //            break;
                                //    //        //case 489435:
                                //    //        //    skipped = 0; // отмена сдвига строк выше
                                //    //        //    break;
                                //    //        default:
                                //    //            indx = indx;
                                //    //            break;
                                //    //    }

                                //    columnLangValues = new Dictionary<string, object>
                                //        {
                                //            { column.name, sData }
                                //        };
                                //    sqlite_db.UpdateRowInTable(table, columnLangValues, $"id = {indx - skipped}");
                                //}
                                //else
                                {
                                    if (!table.ignore)
                                    {
                                        sqlite_db.addToQuery(column.name, sData);
                                    }
                                }
                                break;
                            case "time":
                                var tData = readTime();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, tData);
                                }

                                break;
                            case "blob":
                                var blob = readBlob();
                                if (!table.ignore)
                                {
                                    sqlite_db.addToQuery(column.name, blob, blob.Length);
                                }

                                break;
                            case "color":
                                var data = reader.ReadInt32();
                                if (data != -1)
                                {
                                    //Ghetto Fix for Color Logic
                                    var newName = column.name.Substring(0, column.name.Length - 1);
                                    var r = reader.ReadInt32();
                                    if (!table.ignore)
                                    {
                                        sqlite_db.addToQuery(newName + "r", r);
                                    }

                                    var g = reader.ReadInt32();
                                    if (!table.ignore)
                                    {
                                        sqlite_db.addToQuery(newName + "g", g);
                                    }

                                    var b = reader.ReadInt32();
                                    if (!table.ignore)
                                    {
                                        sqlite_db.addToQuery(newName + "b", b);
                                    }
                                }
                                skips = 2;
                                break;
                            default:
                                using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                                {
                                    sw.WriteLine("Unknown Data Type");
                                }

                                Console.Write("Unknown Data Type");
                                break;
                        }
                    }
                    if (!table.ignore)
                    {
                        sqlite_db.executeQuery();
                    }
                    indx++;
                }
                using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                {
                    if (table.hasCount && rowCount == 0)
                    {
                        sw.WriteLine("Has Empty table " + table.name);
                    }
                    sw.WriteLine("Successfully Parsed " + table.name);
                }
                Console.WriteLine("Successfully Parsed " + table.name);

                if (table.closeDb)
                {
                    using var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192);
                    sw.WriteLine("table.closeDb=true: Let's save the database and finish the job...");
                    break;
                }
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
                    using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                    {
                        sw.WriteLine("Parsing Error Occured on readRow() at offer [{0:x}].", reader.BaseStream.Position);
                    }

                    Console.WriteLine("Parsing Error Occured on readRow() at offer [{0:x}].", reader.BaseStream.Position);
                    Environment.Exit(0);
                    return false;
            }
        }

        static string readCString()
        {
            var data = new StringBuilder();
            char nextChar;

            while ((nextChar = reader.ReadChar()) != '\0')
            {
                data.Append(nextChar);
            }
            return data.ToString();
        }

        static int readInt32()
        {
            return reader.ReadInt32();
        }

        static double readDouble()
        {
            return reader.ReadDouble();
        }
        static float readReal()
        {
            return reader.ReadSingle();
        }

        static bool readBool()
        {
            return reader.ReadBoolean();
        }

        static string readString()
        {
            string data;

            var strType = reader.ReadByte();
            if (strType == 0x2)
            {
                return "";
            }
            else if (strType == 0x1)
            {
                var LTOffset = reader.ReadInt32();

                if (LTOffset == -1)
                {
                    data = readCString();
                    lookup_table.Add(data);
                    return data;
                }
                else
                {
                    try
                    {
                        return lookup_table[LTOffset];
                    }
                    catch (Exception e)
                    {
                        using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                        {
                            sw.WriteLine("Error Parsing String. Unrecognized Type({0}) at offset [{1:x}]", strType, reader.BaseStream.Position);
                        }
                        Console.WriteLine("Error Parsing String. Unrecognized Type({0}) at offset [{1:x}]", strType, reader.BaseStream.Position);
                        throw;
                    }
                }
            }
            else if (strType == 0x0)
            {
                return readCString();
            }
            else
            {
                using (var sw = new StreamWriter(writePath, true, Encoding.Default, bufferSize: 8192))
                {
                    sw.WriteLine("Error Parsing String. Unrecognized Type({0}) at offset [{1:x}]", strType, reader.BaseStream.Position);
                }
                Console.WriteLine("Error Parsing String. Unrecognized Type({0}) at offset [{1:x}]", strType, reader.BaseStream.Position);
                Environment.Exit(0);
                return "";
            }
        }

        static int readColor()
        {
            var data = reader.ReadInt32();
            if (data == -1)
            {
                return -1;
            }
            else
            {
                var r = reader.ReadInt32();
                var g = reader.ReadInt32();
                var b = reader.ReadInt32();
                return r + g + b;
            }
        }

        static DateTime readTime()
        {
            var data = reader.ReadInt64();
            var date = new DateTime(1970, 1, 1).AddSeconds(data);
            return date;
        }

        static byte[] readBlob()
        {
            var size = reader.ReadInt32();
            var data = reader.ReadBytes(size);

            return data;
        }
        static int readInt2()
        {
            var data = reader.ReadInt32();
            switch (data)
            {
                case 5:
                    break;
                case 1:
                    data = reader.ReadInt32();
                    break;
            }

            return data;
        }
    }
}
