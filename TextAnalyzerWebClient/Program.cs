using System;
using System.Net.Sockets;
using System.Text;
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

                while (true)
                {
                    string prefix;
                    bool readRes = ProjectSummer.ReadLineEsc(out prefix);
                    if (readRes && !string.IsNullOrEmpty(prefix))
                    {
                        byte[] data = Encoding.UTF8.GetBytes($"get {prefix}");
                        stream.Write(data, 0, data.Length);
                        stream.Flush();

                        byte[] bytesFrom = new byte[4194304];
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(bytesFrom, 0, bytesFrom.Length);
                            builder.Append(Encoding.UTF8.GetString(bytesFrom, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(builder.ToString());
                        Console.ResetColor();
                    }
                    else
                        break;
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
                if (client != null)
                    client.Close();
            }

        }
    }
}
