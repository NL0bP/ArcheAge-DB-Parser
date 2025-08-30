using System;

namespace ArcheAge_DB_Parser
{

    class Program
    {
        static void Main(string[] args)
        {
            System.IO.File.Delete("export.db");
            System.IO.File.Delete("export.db-journal");
            //System.IO.File.Copy("template.db", "export.db", true);
            //Parser.parse("game", "ja.json");
            //Parser.parse("game", "zh_tw.json");
            //Parser.parse("game10", "unk10.json");
            //Parser.parse("game9", "unk9.json"); // This is Indonesian.
            //Parser.parse("game8", "unk8.json"); // This is Thai - th_th ?
            //Parser.parse("game4", "unk4.json"); // не добавляет, ошибка размера буфера для символа
            //Parser.parse("game0", "ko.json"); // This is correct; it is Korean.

            //Parser.parse("game1.bin", "zh_cn.json", 36); // "ko" in database is wrong, Chinese should be "zh_cn" // для дампа памяти в x32dbg 5070AAFree
            //Parser.parse("game2", "en_us.json");
            Parser.parse("game2.bin", "en_us.json", 36); // для дампа памяти в x32dbg
            //Parser.parse("game4", "zh_cn.json");
            //Parser.parse("game5", "ru.json");
            //Parser.parse("game5.bin", "ru.json", 36); // для дампа памяти в x32dbg
            //Parser.parse("game6", "de.json");
            //Parser.parse("game6.bin", "de.json", 36); // для дампа памяти в x32dbg
            //Parser.parse("game7", "fr.json");
            //Parser.parse("game7.bin", "fr.json", 36); // для дампа памяти в x32dbg
            //Parser.parse("game8", "main_db.json"); // 3+, 5+
            Parser.parse("game8.bin", "main_db.json", 36); // 3+, 5+ // для дампа памяти в x32dbg
            //Parser.parse("game11", "main_db.json"); // 10.8.1.0
            Console.WriteLine("Finished Parsing Data!");
        }
    }
}
