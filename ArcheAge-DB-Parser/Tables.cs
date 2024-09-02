using System.Collections.Generic;

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
        public bool hasCount; // предварительно считывае количество строк для данный таблицы
        public bool ignore;  // игнорируем повторное определение таблицы (данные таблицы в нескольких местах)
        public bool skipServerTable;  // игнорируем серверные таблицы, которые имеются в списке
        public bool closeDb; // завершим работу записав базу (если есть поломанные табоицы в конце базы)
        public string query;//optimization
    }
}
