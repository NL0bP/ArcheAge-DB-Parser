using System;
using System.Collections.Generic;
using System.Text;

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
        public bool ignore;
        public string query;//optimization
    }
}
