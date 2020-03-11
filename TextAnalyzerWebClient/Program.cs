using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fclp;
using TextAnalyzer;

namespace TextAnalyzerWebClient
{
    class Program
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("TextAnalyzerWebClient");
        static void Main(string[] args)
        {
            Logger.Write($"Старт программы с параметрами: {string.Join("; ", args)}");

            for (int j = 0; j < 30; ++j)
            {
                Thread clThread = new Thread(NewClient);
                clThread.Start(args);
                Thread.Sleep(123);
            }
        }

        private static void NewClient(object arg) {
            string[] args = arg as string[];
            if (args == null)
                return;

            TcpClient client = null;
            string address = "";
            int port = 0;
            try
            {
                #region Чтение параметров приложения
                var parConsole = new FluentCommandLineParser();

                parConsole.Setup<string>('a', "address")
                 .Callback(val => address = val)
                 .Required();

                parConsole.Setup<int>('p', "port")
                .Callback(val => port = val)
                .Required();

                var result = parConsole.Parse(args);

                if (result.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Неверные начальные параметры!");
                    Console.ResetColor();
                    Logger.Write("Неверные начальные параметры!");
                    Console.ReadKey();
                    return;
                }
                #endregion

                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();

                for (int i = 0; i < 320; ++i)
                {
                    char pref = ' ';
                    pref = (char)('а' + i % 32);

                    //string pref = Console.ReadLine();
                    string message = "";
                    byte[] data = Encoding.UTF8.GetBytes($"get {pref}");
                    stream.Write(data, 0, data.Length);
                    stream.Flush();

                    byte[] bytesFrom = new byte[4194304];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    int count = 0;
                    do
                    {
                        bytes = stream.Read(bytesFrom, 0, bytesFrom.Length);
                        builder.Append(Encoding.UTF8.GetString(bytesFrom, 0, bytes));
                        ++count;
                    }
                    while (stream.DataAvailable);

                    message = builder.ToString();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ResetColor();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                string description = "";
                if (ex is SocketException)
                {
                    description = (ex as SocketException).SocketErrorCode.ToString();
                }
                Console.WriteLine($"Ошибка в программе!{description}");
                Console.ResetColor();
                Console.ReadKey();
            }
            finally
            {
                client.Close();
            }

        }
    }
}
