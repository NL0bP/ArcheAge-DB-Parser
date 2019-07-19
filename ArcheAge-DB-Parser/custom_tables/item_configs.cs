using System;
using System.Collections.Generic;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        class item_configs
        {
            public static void parse()
            {
                readRow();
                double durability_decrement_chance = readDouble();
                double durability_repair_cost_factor = readDouble();
                double durability_const = readDouble();
                double holdable_durability_const = readDouble();
                double wearable_durability_const = readDouble();
                int death_durability_loss_ratio = readInt32();
                int item_stat_const = readInt32();
                int holdable_stat_const = readInt32();
                int wearable_stat_const = readInt32();
                int stat_value_const = readInt32();
            }
        }
    }
}