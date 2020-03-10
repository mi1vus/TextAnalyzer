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
                //foreach (var i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                //    foreach (var ua in i.GetIPProperties().UnicastAddresses)
                //        Console.WriteLine(ua.Address);
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
                    //для кеширования запроса
                    TextParser.GetNearWords("a", false);

                    Thread clThread = new Thread(Listen);
                    clThread.Start(port);

                    #region Чтение команд с консоли
                    string command = "";
                    while (true)
                    {
                        command = Console.ReadLine();
                        Logger.Write($"Введена команда: {command}");
                        if (!string.IsNullOrEmpty(command))
                        {
                            var commands = command.Split(new[] { '\r', '\n', ' ', '\'', '"'}, StringSplitOptions.RemoveEmptyEntries);

                            bool noError = false;
                            if (commands.Length > 0)
                            {
                                switch (commands[0])
                                {
                                    case "-add":
                                        noError = commands.Length == 2 ?
                                            TextParser.ParseTextToDB(commands[1], false):
                                            false;
                                        Logger.Write($"Результат add: {noError}");
                                        break;
                                    case "-update":
                                        noError = commands.Length == 2 ?
                                            TextParser.ParseTextToDB(commands[1], true) :
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

                    while (true)
                    {
                        counter += 1;
                        clientSocket = serverSocket.AcceptTcpClient();
                        Logger.Write($"Подключение клиента[{counter}]");
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
                    //Console.ReadLine();
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
                    Thread clThread = new Thread(HandleRequest);
                    clThread.Start();
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
                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse;

                while (ClientSocket.Connected)
                {
                    try
                    {
                        serverResponse = "";
                        ++requestCount;
                        NetworkStream stream = ClientSocket.GetStream();
                        //stream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        // получаем ответ
                        //byte[] data = new byte[64]; // буфер для получаемых данных
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(bytesFrom, 0, bytesFrom.Length);
                            builder.Append(Encoding.UTF8.GetString(bytesFrom, 0, bytes));
                        }
                        while (stream.DataAvailable);

                        dataFromClient = builder.ToString();

                        //dataFromClient = System.Text.Encoding.UTF8.GetString(bytesFrom);
                        //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf($"${(char)0}"));
                        //TODO прекратить при обрыве клиента
                        Logger.Write($"Запрос клиента[{Id}]:{dataFromClient}");

                        var commands = dataFromClient.Split(new[] { '\r', '\n', ' ', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);

                        if (commands.Length == 2 && string.Compare(commands[0], "get") == 0)
                        {
                            serverResponse = string.Join(Environment.NewLine, TextParser.GetNearWords(commands[1].ToLower()));
                        }

                        //serverResponse = "Server to clinet(" + Id + ") ";
                        sendBytes = Encoding.UTF8.GetBytes(serverResponse);
                        stream.Write(sendBytes, 0, sendBytes.Length);
                        stream.Flush();
                        Logger.Write($"Ответ сервера клиенту[{Id}]:\r\n{serverResponse}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Write($"Ошибка в HandleRequest:{ex.ToString()}");
                    }
                }
            }
        }
    }
}