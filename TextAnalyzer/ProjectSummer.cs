using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace TextAnalyzer
{
    public class ProjectSummer
    {
        private static Logger Logger_ = new Logger("ProjectSummer");

        public class Logger
        {
            public Logger(string name)
            {
                LoggerName = name;
            }

            #region Приватные переменные класса
            private static string LogDirPrivate = AppDomain.CurrentDomain.BaseDirectory;
            private string PathMem = "";
            private DateTime DateMem = DateTime.MinValue;
            private string PathS() => PathS(DateTime.Today);
            private string PathS(DateTime DateAddon)
            {
                if (DateMem != DateAddon)
                {
                    DateMem = DateAddon;
                    PathMem = Path.Combine(LogDirPrivate, "logs", $"{DateTime.Today.Year:0000}{DateTime.Today.Month:00}{DateTime.Today.Day:00}");
                }
                return PathMem;

            }
            private string FileName = "";

            #endregion

            #region Публичные свойства класса
            /// <summary>
            /// Путь до директории для хранения лог файлов
            /// </summary>
            public static string LogDir
            {
                get
                {
                    return LogDirPrivate;
                }
                set
                {
                    LogDirPrivate = Path.GetFullPath(value);
                }
            }


            /// <summary>
            /// Имя экземпляра логера
            /// </summary>
            public string LoggerName
            {
                get;
                private set;
            }
            #endregion

            #region Функции записи информации в лог

            /// <summary>
            /// Добавление записи в лог, с возможностью форматирования.
            /// Аналог string.Format()
            /// </summary>
            /// <param name="format">Строка составного форматирования</param>
            /// <param name="args">Объекты для форматирования</param>
            /// <returns></returns>
            public bool WriteFormated(string format, params object[] args)
            {
                return Write(string.Format(format, args));
            }
            /// <summary>
            /// Запись в лог сообщения с отметкой времени
            /// </summary>
            /// <param name="message">Текст записи</param>
            /// <returns>False - если возникла ошибка</returns>
            public bool Write(string message)
            {
                if (string.IsNullOrEmpty(message))
                    return false;

                try
                {
                    var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    string line = "";

                    for (int z = 0; z < lines.Length; z++)
                    {
                        line += string.Format("{0:dd/MM/yy HH:mm:ss.fff}>{1}" + (z + 1 != lines.Length ? Environment.NewLine : ""), DateTime.Now, lines[z]);
                    }

                    if (!System.IO.Directory.Exists(PathS()))
                        System.IO.Directory.CreateDirectory(PathS());
                    var today = DateTime.Today;
                    if (FileName != GetFileName(DateTime.Today))
                        FileName = GetFileName(DateTime.Today);
                    lock (FileName)
                    {
                        using (FileStream fileSteam = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete | FileShare.Read | FileShare.Write))
                        {
                            using (System.IO.StreamWriter str = new StreamWriter(fileSteam, Encoding.UTF8))
                            {
                                str.WriteLine(line);
                                str.Flush();
                            }
                            fileSteam.Close();
                        }
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }

            #endregion

            #region Вспомогательные методы
            /// <summary>
            /// Старт таймера для оценки времени работы программы
            /// </summary>
            public void StartTimer()
            {
                stopWatch.Start();
            }
            /// <summary>
            /// Останова и сброс таймера
            /// </summary>
            public void ResetTimer()
            {
                stopWatch.Stop();
                stopWatch.Reset();
            }
            /// <summary>
            /// Добавление записи в лог, с возможностью форматирования.
            /// Аналог string.Format()
            /// </summary>
            /// <param name="format">Строка составного форматирования</param>
            /// <param name="args">Объекты для форматирования</param>
            /// <returns></returns>
            public bool LogTimerAndRestartFormated(string format, params object[] args)
            {
                return LogTimerAndRestart(string.Format(format, args));
            }
            /// <summary>
            /// Запись в лог с отметкой текущего значения таймера в конце строки, обнулением таймера и его стартом.
            /// </summary>
            /// <param name="msg">Текст записи</param>
            public bool LogTimerAndRestart(string msg)
            {
                bool result = false;
                try
                {
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;

                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds);
                    result = Write($"{msg} {elapsedTime}");

                    stopWatch.Reset();
                    stopWatch.Start();
                    return result;
                }
                catch
                {
                    return false;
                }
            }
            private Stopwatch stopWatch = new Stopwatch();

            private string get_file_name = "";
            private DateTime get_filename_date = DateTime.MinValue;
            private string GetFileName(DateTime dateAddon)
            {
                if (dateAddon != get_filename_date)
                {
                    if (!Directory.Exists(PathS(dateAddon)))
                        Directory.CreateDirectory(PathS(dateAddon));
                    string tmpFileName = string.Format($"{LoggerName}_{dateAddon.Year:0000}{dateAddon.Month:00}{dateAddon.Day:00}.log");
                    get_file_name = Path.Combine(PathS(dateAddon), tmpFileName);
                }
                return get_file_name;
            }
            #endregion
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
                    #region Нажат back
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
                    #endregion
                    #region Нажат del
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
                    #endregion
                    #region Введен символ
                    else if (Char.IsLetterOrDigit(key.KeyChar) || Char.IsWhiteSpace(key.KeyChar))
                    {
                        var cli = Console.CursorLeft;
                        buffer.Insert(cli, key.KeyChar);
                        Console.CursorLeft = 0;
                        Console.Write(buffer.ToString());
                        Console.CursorLeft = cli + 1;
                        key = Console.ReadKey(true);
                    }
                    #endregion
                    #region Нажата стрелка влево
                    else if (key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > 0)
                    {
                        Console.CursorLeft--;
                        key = Console.ReadKey(true);
                    }
                    #endregion
                    #region Нажата стрелка вправо
                    else if (key.Key == ConsoleKey.RightArrow && Console.CursorLeft < buffer.Length)
                    {
                        Console.CursorLeft++;
                        key = Console.ReadKey(true);
                    }
                    #endregion
                    #region Ожидание следующего нажатия
                    else
                    {
                        key = Console.ReadKey(true);
                    } 
                    #endregion
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
                Logger_.Write($"Ошибка в ReadLineEsc: {ex.ToString()}");
                return false;
            }

            return false;
        }
    }

}
