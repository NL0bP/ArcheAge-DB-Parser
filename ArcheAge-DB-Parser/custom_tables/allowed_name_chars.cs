using System;
using System.Collections.Generic;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        class allowed_name_chars
        {
            public static void parse()
            {
                int id = 0;

                readRow();
                int count = readInt32();
                int max = readInt32();
                int sum = readInt32();
                readRow();

                while (readRow())
                {
                    string char_val = readString();
                    int char_len = readInt32();
                    id++;
                }
            }
        }
    }
}