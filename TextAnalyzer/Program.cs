using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            bool error = true;
            if (args.Count() > 0)
            {
                if (args.Count() == 2)
                {
                    switch (args[0])
                    {
                        case "add":
                            TextParser.ParseText(args[1], false);
                            error = false;
                            break;
                        case "append":
                            TextParser.ParseText(args[1], true);
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
