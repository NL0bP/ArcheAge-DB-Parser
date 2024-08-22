using System.Collections.Generic;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        static bool first_call = true;
        class wearables_row
        {
            public int armor_bp;
            public int magic_bp;
            public int armor_type_id;
            public int slot_type_id;
        }

        static List<wearables_row> rows = new List<wearables_row>();

        class wearables
        {
            public static void parse(Table table)
            {
                int count = 0;
                while (readRow())
                {
                    wearables_row new_row = new wearables_row();
                    if (first_call)
                    {
                        rows.Add(new_row);
                    }

                    int bp = reader.ReadInt32();
                    if (first_call)
                    {
                        rows[count].armor_bp = bp;
                    }
                    else
                    {
                        rows[count].magic_bp = bp;
                    }

                    int armor_type_id = reader.ReadInt32();
                    rows[count].armor_type_id = armor_type_id;
                    int slot_type_id = reader.ReadInt32();
                    rows[count].slot_type_id = slot_type_id;
                    count++;
                }
                if (!first_call)
                {
                    Column col = new Column();
                    col.name = "armor_bp";
                    col.type = "int";
                    table.columns.Insert(0, col);
                    sqlite_db.createTable(table);

                    foreach (wearables_row row in rows)
                    {
                        sqlite_db.createQuery(table);
                        sqlite_db.addToQuery("armor_bp", row.armor_bp);
                        sqlite_db.addToQuery("magic_resistance_bp", row.magic_bp);
                        sqlite_db.addToQuery("armor_type_id", row.armor_type_id);
                        sqlite_db.addToQuery("slot_type_id", row.slot_type_id);
                        sqlite_db.executeQuery();
                    }
                }
                first_call = false;
            }
        }
    }
}
