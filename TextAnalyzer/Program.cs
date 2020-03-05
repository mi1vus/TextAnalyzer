using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextAnalyzer.DBHelper;

namespace TextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO удалить
            //Database.SetInitializer<WordsContext>(null);

            bool error = true;
            if (args.Count() > 0)
            {
                if (args.Count() == 2)
                {
                    switch (args[0])
                    {
                        case "add":
                            TextParser.ParseTextToDB(args[1], false);
                            error = false;
                            break;
                        case "append":
                            TextParser.ParseTextToDB(args[1], true);
                            error = false;
                            break;
                        //default:
                        //    error = true;
                        //    break;
                    }
                }
                if (error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong parameters!");
                    Console.ResetColor();
                }
            }
            else
            {

            }
        }
    }
}
