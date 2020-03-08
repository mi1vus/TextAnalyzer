using System;
using System.Linq;
using System.Text;

namespace TextAnalyzer
{
    class Program
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("MainProgram");

        static void Main(string[] args)
        {
            Logger.Write($"Старт программы с параметрами: {string.Join(", ", args)}");
                
            if (args.Count() > 0)
            {
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
                }
            }
            else
            {
                //для кеширования запроса
                TextParser.GetNearWords("a", false);

                while (true)
                {
                    string prefix = "";
                    bool readRes = ReadLineEsc(out prefix);
                    Logger.Write($"Введено слово: {prefix}");
                    if (readRes && !string.IsNullOrEmpty(prefix))
                    {
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
            }
        }

        /// <summary>
        /// Прочитать следующую строку из консоли, выход по esc
        /// </summary>
        /// <param name="line">В эту переменную помещается прочтенная строка</param>
        /// <returns></returns>
        public static bool ReadLineEsc(out string line)
        {
            line = string.Empty;
            try
            {
                var buffer = new StringBuilder();
                var key = Console.ReadKey(true);
                while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
                {
                    if (key.Key == ConsoleKey.Backspace && Console.CursorLeft > 0)
                    {
                        var cli = --Console.CursorLeft;
                        buffer.Remove(cli, 1);
                        Console.CursorLeft = 0;
                        Console.Write(new String(Enumerable.Range(0, buffer.Length + 1).Select(o => ' ').ToArray()));
                        Console.CursorLeft = 0;
                        Console.Write(buffer.ToString());
                        Console.CursorLeft = cli;
                        key = Console.ReadKey(true);
                    }
                    else if (key.Key == ConsoleKey.Delete && Console.CursorLeft < buffer.Length)
                    {
                        var cli = Console.CursorLeft;
                        buffer.Remove(cli, 1);
                        Console.CursorLeft = 0;
                        Console.Write(new String(Enumerable.Range(0, buffer.Length + 1).Select(o => ' ').ToArray()));
                        Console.CursorLeft = 0;
                        Console.Write(buffer.ToString());
                        Console.CursorLeft = cli;
                        key = Console.ReadKey(true);
                    }
                    else if (Char.IsLetterOrDigit(key.KeyChar) || Char.IsWhiteSpace(key.KeyChar))
                    {
                        var cli = Console.CursorLeft;
                        buffer.Insert(cli, key.KeyChar);
                        Console.CursorLeft = 0;
                        Console.Write(buffer.ToString());
                        Console.CursorLeft = cli + 1;
                        key = Console.ReadKey(true);
                    }
                    else if (key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > 0)
                    {
                        Console.CursorLeft--;
                        key = Console.ReadKey(true);
                    }
                    else if (key.Key == ConsoleKey.RightArrow && Console.CursorLeft < buffer.Length)
                    {
                        Console.CursorLeft++;
                        key = Console.ReadKey(true);
                    }
                    else
                    {
                        key = Console.ReadKey(true);
                    }
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    line = buffer.ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Ошибка в ReadLineEsc: {ex.ToString()}");
                return false;
            }

            return false;
        }
    }
}
