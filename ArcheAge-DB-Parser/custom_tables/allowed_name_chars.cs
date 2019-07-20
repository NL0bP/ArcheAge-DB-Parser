using System;
using System.Collections.Generic;
using System.Text;

namespace ArcheAge_DB_Parser
{
    partial class Parser
    {
        class allowed_name_chars
        {
            public static void parse(Table table)
            {
                int id = 1;
                sqlite_db.createTable(table);
                readRow();
                int count = readInt32();
                int max = readInt32();
                int sum = readInt32();
                readRow();

                while (readRow())
                {
                    sqlite_db.createQuery(table);

                    sqlite_db.addToQuery("id", id);
                    string char_val = readString();
                    sqlite_db.addToQuery("char", char_val);
                    int char_len = readInt32();
                    sqlite_db.addToQuery("bytes", char_len);

                    sqlite_db.executeQuery();
                    id++;
                }
            }
        }
    }
}