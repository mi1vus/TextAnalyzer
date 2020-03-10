using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Fclp;
using TextAnalyzer;

namespace TextAnalyzerWebServer
{
    public class WebServer
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("TextAnalyzerWebServer");

        class Program
        {
            static void Main(string[] args)
            {
                Logger.Write($"Старт программы с параметрами: {string.Join("; ", args)}");
                string connectionString = "";
                int port = 0;
                try
                {
                    #region Чтение параметров приложения
                    var parConsole = new FluentCommandLineParser();

                    parConsole.Setup<string>('c', "connection")
                     .Callback(val => connectionString = val)
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

                    TextParser.Initialize(connectionString);
                    //для кэширования запроса
                    TextParser.GetNearWords("a", false);

                    Thread clientThread = new Thread(Listen);
                    clientThread.Start(port);

                    #region Чтение команд с консоли
                    string command = "";
                    while (true)
                    {
                        command = Console.ReadLine();
                        Logger.Write($"Введена команда: {command}");
                        if (!string.IsNullOrEmpty(command))
                        {
                            var commands = command.Split(new[] { '\'', '"'}, StringSplitOptions.RemoveEmptyEntries);
                            
                            bool noError = false;
                            if (commands.Length > 0)
                            {
                                switch (commands[0].Trim())
                                {
                                    case "-add":
                                        noError = commands.Length == 2 ?
                                            TextParser.ParseTextToDB(commands[1].Trim(), false):
                                            false;
                                        Logger.Write($"Результат add: {noError}");
                                        break;
                                    case "-update":
                                        noError = commands.Length == 2 ?
                                            TextParser.ParseTextToDB(commands[1].Trim(), true) :
                                            false;
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
                                Console.WriteLine("Введена неверная команда!");
                                Console.ResetColor();
                                Logger.Write("Введена неверная команда!");
                            }
                        }
                    } 
                    #endregion
                }
                catch (Exception ex)
                {
                    Logger.Write($"Ошибка в Main: {ex.ToString()}");
                }
            }

            private static void Listen(object port)
            {
                if (!(port is int))
                    return;

                TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, (int)port);
                TcpClient clientSocket = default(TcpClient);
                int counter = 0;
                try
                {
                    serverSocket.Start();
                    Logger.Write("Начат прием соединений");
                    Console.WriteLine("Старт сервера");

                    while (true)
                    {
                        counter += 1;
                        clientSocket = serverSocket.AcceptTcpClient();
                        Logger.Write($"Подключение клиента[{counter}]");
                        //  Класс представляющий новое подключение
                        HandleClinet client = new HandleClinet();
                        client.Start(clientSocket, counter.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write($"Ошибка в сетевом обмене: {ex.ToString()}");
                }
                finally
                {
                    clientSocket.Close();
                    serverSocket.Stop();
                    Logger.Write("Остановка сервера");
                    Console.WriteLine("Остановка сервера");
                }
            }
        }

        public class HandleClinet
        {
            TcpClient ClientSocket;
            string Id;
            public bool Start(TcpClient inClientSocket, string clineNo)
            {
                try
                {
                    this.ClientSocket = inClientSocket;
                    this.Id = clineNo;
                    Thread clientThread = new Thread(HandleRequest);
                    clientThread.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Write($"Ошибка в HandleClinet.Start: {ex.ToString()}");
                    return false;
                }
            }
            private void HandleRequest()
            {
                int requestCount = 0;
                byte[] bytesFrom = new byte[16384];
                string dataFromClient = null;
                byte[] sendBytes = null;
                string serverResponse;

                while (ClientSocket.Connected)
                {
                    try
                    {
                        ++requestCount;
                        NetworkStream stream = ClientSocket.GetStream();
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(bytesFrom, 0, bytesFrom.Length);
                            builder.Append(Encoding.UTF8.GetString(bytesFrom, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        dataFromClient = builder.ToString();

                        Logger.Write($"Запрос клиента[{Id}]:{dataFromClient}");

                        var commands = dataFromClient.Split(new[] { '\r', '\n', ' ', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);

                        serverResponse = "";
                        int responseSize = 0;
                        if (commands.Length == 2 && string.Compare(commands[0], "get") == 0)
                        {
                            var words = TextParser.GetNearWords(commands[1].ToLower(), true, 0);
                            responseSize = words.Count;
                            serverResponse = string.Join(Environment.NewLine, words);
                        }
                        if (string.IsNullOrEmpty(serverResponse))
                            serverResponse = "-";

                        dataFromClient = "";

                        sendBytes = Encoding.UTF8.GetBytes(serverResponse);
                        stream.Write(sendBytes, 0, sendBytes.Length);
                        stream.Flush();
                        Logger.Write($"Ответ сервера клиенту[{Id}]:{responseSize} слов");
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Отсоединение клиента[{Id}]");
                        Logger.Write($"Ошибка в HandleRequest:{ex.ToString()}");
                    }
                }
            }
        }
    }
}