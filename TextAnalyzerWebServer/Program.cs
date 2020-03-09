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

                    Thread clThread = new Thread(Listen);
                    clThread.Start(port);

                    #region Чтение команд с консоли
                    var parCommandLine = new FluentCommandLineParser();

                    bool add, update, clear;
                    string filePath;
                    parCommandLine.Setup<string>('a', "add")
                     .Callback(val => { filePath = val; add = true; });

                    parCommandLine.Setup<string>('u', "update")
                     .Callback(val => { filePath = val; update = true; });

                    parCommandLine.Setup<bool>('c', "clear")
                     .SetDefault(false).
                     .Callback(val => clear = val);

                    string command = "";
                    while (true)
                    {
                        add = false; update = false; clear = false;
                        filePath = "";
                        command = Console.ReadLine();//ProjectSummer.ReadLineEsc(out command);
                        Logger.Write($"Введена команда: {command}");
                        if (!string.IsNullOrEmpty(command))
                        {
                            result = parCommandLine.Parse(command.Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                            if (result.HasErrors)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Неверные формат команды!");
                                Console.ResetColor();
                                Logger.Write("Неверные формат команды!");
                            }
                            else
                            {
                                bool noError = false;
                                if (clear)
                                {
                                    noError = TextParser.ClearDB();
                                    Logger.Write($"Результат clear: {noError}");
                                }
                                if (add && !string.IsNullOrWhiteSpace(filePath))
                                {
                                    noError = TextParser.ParseTextToDB(filePath, false);
                                    Logger.Write($"Результат add: {noError}");
                                }
                                if (update && !string.IsNullOrWhiteSpace(filePath))
                                {
                                    noError = TextParser.ParseTextToDB(filePath, true);
                                    Logger.Write($"Результат update: {noError}");
                                }
                            }
                            //var words = TextParser.GetNearWords(command);
                            //if (words.Count > 0)
                            //{
                            //    Console.ForegroundColor = ConsoleColor.Green;
                            //    Console.WriteLine(string.Join(Environment.NewLine, words));
                            //    Console.ResetColor();
                            //}
                        }
                        //else
                        //    break;
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
                    Logger.Write("Server Started");

                    while (true)
                    {
                        counter += 1;
                        clientSocket = serverSocket.AcceptTcpClient();
                        Logger.Write($"Client[{counter}] started!");
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
                    Console.WriteLine(" >> " + "exit");
                    Console.ReadLine();
                }
            }
        }


        //Class to handle each client request separatly
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
                string serverResponse = null;
                string rCount = null;
                requestCount = 0;

                while (ClientSocket.Connected)
                {
                    try
                    {
                        requestCount = requestCount + 1;
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
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf($"${(char)0}"));
                        Console.WriteLine(" >> " + "From client-" + Id + dataFromClient);

                        rCount = Convert.ToString(requestCount);
                        serverResponse = "Server to clinet(" + Id + ") " + rCount;
                        sendBytes = Encoding.UTF8.GetBytes(serverResponse);
                        stream.Write(sendBytes, 0, sendBytes.Length);
                        stream.Flush();
                        Console.WriteLine(" >> " + serverResponse);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" >> " + ex.ToString());
                    }
                }
            }
        }
    }
}