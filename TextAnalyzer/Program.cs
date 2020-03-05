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
                            //TODO удалить
                            TextParser.ParseTextToDB(args[1], false);
                            error = false;
                            break;
                        case "update":
                            TextParser.ParseTextToDB(args[1], true);
                            //TODO удалить
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
                //для кеширования запроса
                TextParser.GetNearWords("a");
                ConsoleKeyInfo btn;
                string prefix = "";
                while (true)
                {
                    prefix = "";
                    do
                    {
                        btn = Console.ReadKey();
                        prefix += btn.KeyChar;
                    }
                    while (btn.Key != ConsoleKey.Escape
                            && btn.Key != ConsoleKey.Enter);

                    prefix = prefix.Substring(0, prefix.Length - 1);

                    if (btn.Key != ConsoleKey.Escape && !string.IsNullOrEmpty(prefix))
                    {
                        var words = TextParser.GetNearWords(prefix);
                        if (words.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, words));
                            Console.ResetColor();
                        }
                        else
                            Console.WriteLine("");
                    }
                    else
                        break;
                }
            }
        }
    }
}
