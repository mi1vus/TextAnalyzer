  using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalyzerWebClient
{
    class Program
    {
        const int port = 1234;
        const string address = "localhost";
        const string address2 = "127.0.0.1";
        static void Main(string[] args)
        {
            //Console.Write("Введите свое имя:");
            //string userName = Console.ReadLine();
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    //Console.Write(userName + ": ");
                    //// ввод сообщения
                    string message = Console.ReadLine();
                    //message = String.Format("{0}: {1}$\0", userName, message);
                    //var message = $"Макс: Привет неудачники!${(char)0}";
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.UTF8.GetBytes($"get {message}");
                    // отправка сообщения
                    stream.Write(data, 0, data.Length);

                    // получаем ответ
                    data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    message = builder.ToString();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ResetColor();
                    //Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
