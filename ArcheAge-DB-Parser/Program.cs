using System;

namespace ArcheAge_DB_Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            System.IO.File.Delete("export.db");
            //System.IO.File.Delete("export.db-journal");
            //System.IO.File.Copy("template.db", "export.db", true);
            Parser.parse("game2", "en_us.json");
            //Parser.parse("game5", "ru.json");
            //Parser.parse("game6", "de.json");
            //Parser.parse("game7", "fr.json");
            Parser.parse("game8", "main_db.json");
            Console.WriteLine("Finished Parsing Data!");
        }
    }
}
