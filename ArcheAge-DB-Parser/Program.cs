using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArcheAge_DB_Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            Parser.parse("main_db.bin", "db.json");
            Console.WriteLine("Finished Parsing Data!");
        }
    }
}
