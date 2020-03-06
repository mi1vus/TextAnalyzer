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
            Console.OutputEncoding = Encoding.UTF8;
            bool error = true;
            if (args.Count() > 0)
            {
                if (args.Count() == 2)
                {
                    //TODO удалить
                    //TextParser.ParseTextToDB(args[1], false);
                    //TextParser.ParseTextToDB(args[1], false);
                    //TextParser.ParseTextToDB(args[1], true);
                    //TextParser.ParseTextToDB(args[1], true);

                    switch (args[0])
                    {
                        case "-add":
                            TextParser.ParseTextToDB(args[1], false);
                            //TODO удалить
                            //TextParser.ParseTextToDB(args[1], false);
                            error = false;
                            break;
                        case "-update":
                            TextParser.ParseTextToDB(args[1], true);
                            //TODO удалить
                            //TextParser.ParseTextToDB(args[1], true);
                            error = false;
                            break;
                        case "-clear":
                            TextParser.ParseTextToDB(args[1], true);
                            //TODO удалить
                            //TextParser.ParseTextToDB(args[1], true);
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
                //TODO Удалить
                //while(true)
                //    Console.Read();
                int i = 0;

                //для кеширования запроса
                TextParser.GetNearWords("a", false);
                ConsoleKeyInfo btn;
                string prefix = "";
                while (true)
                {
                    prefix = "";
                    while (true)
                    {
                        var r = Console.ReadLine();
                        btn = Console.ReadKey(false);
                        //i = Console.Read();
                        //if (btn.Key == ConsoleKey.Escape
                        //    || btn.Key == ConsoleKey.Enter)
                        //    break;
                        //else if (btn.Key == ConsoleKey.Backspace)
                        //{
                        //    if (prefix.Length > 0)
                        //        prefix = prefix.Substring(0, prefix.Length - 1);
                        //    continue;
                        //}
                        //else
                        //    prefix += btn.KeyChar;
                        //if (btn.Key != ConsoleKey.Escape && !string.IsNullOrEmpty(prefix))
                        //{

                        if (i == (int)ConsoleKey.Escape
                            || i == (int)ConsoleKey.Enter)
                            break;
                        else if (i == (int)ConsoleKey.Backspace)
                        {
                            if (prefix.Length > 0)
                                prefix = prefix.Substring(0, prefix.Length - 1);
                            continue;
                        }
                        else
                            prefix += (char)i;
                    }

                    if (i != (int)ConsoleKey.Escape && !string.IsNullOrEmpty(prefix))
                    {
                        var words = TextParser.GetNearWords(prefix);
                        if (words.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(Environment.NewLine + string.Join(Environment.NewLine, words));
                            //TODO Удалить
                            //Console.WriteLine(string.Join(Environment.NewLine, TextParser.GetNearWordsNoOrder(prefix)));
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
