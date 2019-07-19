using System;
using System.Collections.Generic;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        class wearable_slots
        {
            public static void parse()
            {
                while (readRow())
                {
                    int slot_type_id = reader.ReadInt32();
                    if ((slot_type_id == 1) || ((2 < slot_type_id && (slot_type_id < 9))))
                    {
                        int coverage = reader.ReadInt32();
                    }
                }
            }
        }
    }
}
