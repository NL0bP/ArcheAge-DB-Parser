using System;
using System.Collections.Generic;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        class item_configs
        {
            public static void parse(Table table)
            {
                readRow();
                sqlite_db.createTable(table);
                sqlite_db.createQuery(table);

                double durability_decrement_chance = readDouble();
                sqlite_db.addToQuery("durability_decrement_chance", durability_decrement_chance);
                double durability_repair_cost_factor = readDouble();
                sqlite_db.addToQuery("durability_repair_cost_factor", durability_repair_cost_factor);
                double durability_const = readDouble();
                sqlite_db.addToQuery("durability_const", durability_const);
                double holdable_durability_const = readDouble();
                sqlite_db.addToQuery("holdable_durability_const", holdable_durability_const);
                double wearable_durability_const = readDouble();
                sqlite_db.addToQuery("wearable_durability_const", wearable_durability_const);
                int death_durability_loss_ratio = readInt32();
                sqlite_db.addToQuery("death_durability_loss_ratio", death_durability_loss_ratio);
                int item_stat_const = readInt32();
                sqlite_db.addToQuery("item_stat_const", item_stat_const);
                int holdable_stat_const = readInt32();
                sqlite_db.addToQuery("holdable_stat_const", holdable_stat_const);
                int wearable_stat_const = readInt32();
                sqlite_db.addToQuery("wearable_stat_const", wearable_stat_const);
                int stat_value_const = readInt32();
                sqlite_db.addToQuery("stat_value_const", stat_value_const);

                sqlite_db.executeQuery();
            }
        }
    }
}