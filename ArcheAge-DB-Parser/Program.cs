using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArcheAge_DB_Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            //System.IO.File.Copy("template.db", "export.db", true);
            Parser.parse("en_us.bin", "en_us.json");
            //Parser.parse("main_db.bin", "main_db.json");
            Console.WriteLine("Finished Parsing Data!");
        }
    }
}
