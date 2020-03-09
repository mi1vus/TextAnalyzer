using System;
using System.Linq;
using System.Text;

namespace TextAnalyzer
{
    class Program
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("TextAnalyzerLocal");

        static void Main(string[] args)
        {
            Logger.Write($"Старт программы с параметрами: {string.Join("; ", args)}");
            TextParser.Initialize("name=Test2");
            if (args.Count() > 0)
            {
                #region Модификация словаря
                bool noError = false;
                if (args.Count() == 2)
                {
                    switch (args[0])
                    {
                        case "-add":
                            noError = TextParser.ParseTextToDB(args[1], false);
                            Logger.Write($"Результат add: {noError}");
                            break;
                        case "-update":
                            noError = TextParser.ParseTextToDB(args[1], true);
                            Logger.Write($"Результат update: {noError}");
                            break;
                        case "-clear":
                            noError = TextParser.ClearDB();
                            Logger.Write($"Результат clear: {noError}");
                            break;
                    }
                }
                if (!noError)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Неверные начальные параметры!");
                    Console.ResetColor();
                    Logger.Write("Неверные начальные параметры!");
                    Console.ReadKey();
                    return;
                } 
                #endregion
            }
            else
            {
                #region Запросы к словарю
                //для кеширования запроса
                TextParser.GetNearWords("a", false);
                string prefix = "";
                while (true)
                {
                    bool readRes = ProjectSummer.ReadLineEsc(out prefix);
                    Logger.Write($"Введено слово: {prefix}");
                    if (readRes && !string.IsNullOrEmpty(prefix))
                    {
                        prefix = prefix.ToLower();
                        var words = TextParser.GetNearWords(prefix);
                        if (words.Count > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Join(Environment.NewLine, words));
                            Console.ResetColor();
                        }
                    }
                    else
                        break;
                }

                #endregion
            }
        }
    }
}
