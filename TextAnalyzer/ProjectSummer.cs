﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TextAnalyzer
{
    class ProjectSummer
    {
        //TODO почистить
        public class Logger
        {
            public Logger(string name)
            {
                LoggerName = name;
            }

            #region Приватные переменные класса
            private static string _logDir = AppDomain.CurrentDomain.BaseDirectory;
            private string path_mem = "";
            private DateTime date_mem = DateTime.MinValue;

            private string path() => path(DateTime.Today);
            private string path(DateTime DateAddon)
            {
                if (date_mem != DateAddon)
                {
                    date_mem = DateAddon;
                    path_mem = Path.Combine(_logDir, "logs", $"{DateTime.Today.Year:0000}{DateTime.Today.Month:00}{DateTime.Today.Day:00}");
                }
                return path_mem;

            }

            #endregion

            #region Публичные свойства класса
            /// <summary>
            /// Путь до директории для хранения лог файлов
            /// </summary>
            public static string LogDir
            {
                get
                {
                    return _logDir;
                }
                set
                {
                    _logDir = Path.GetFullPath(value);
                }
            }

            /// <summary>
            /// Дата/время последней отчистки архива
            /// </summary>
            public DateTime LastArchiveClear
            {
                get;
                private set;
            }

            /// <summary>
            /// Текущий уровень логирования
            /// </summary>
            public int LogLevel
            {
                get;
                set;
            }

            /// <summary>
            /// Включение/отключение записи логов на диск
            /// </summary>
            public bool LogEnable
            {
                get;
                set;
            }

            /// <summary>
            /// Глубина архива логов (дней)
            /// </summary>
            public int ArchiveDepth
            {
                get;
                set;
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

            public bool Write(string message)
            {
                if (string.IsNullOrEmpty(message))
                    return false;

                var lines = message.Split('\n');
                string line = "";
                for (int z = 0; z < lines.Length; z++)
                    line += string.Format("{0:dd/MM/yy HH:mm:ss.fff}>{2}" + (z + 1 != lines.Length ? "\n" : ""), DateTime.Now, lines[z]);

                return true;
            }

            #endregion

            #region Вспомогательные методы
            private string get_file_name = "";
            private DateTime get_filename_date = DateTime.MinValue;
            private string GetFileName(DateTime dateAddon)
            {
                if (dateAddon != get_filename_date)
                {
                    if (!Directory.Exists(path(dateAddon)))
                        Directory.CreateDirectory(path(dateAddon));
                    string tmpFileName = string.Format($"{LoggerName}_{dateAddon.Year:0000}{dateAddon.Month:00}{dateAddon.Day:00}.log");
                    get_file_name = Path.Combine(path(dateAddon), tmpFileName);
                }
                return get_file_name;
            }
            public string GetDirName(DateTime dateAddon)
            {
                if (!Directory.Exists(path(dateAddon)))
                    Directory.CreateDirectory(path(dateAddon));
                return path(dateAddon);
            }
            private string readDay(DateTime dTime)
            {
                string strret = "";
                try
                {
                    if (!System.IO.Directory.Exists(path(dTime)))
                        System.IO.Directory.CreateDirectory(path(dTime));
                    string fileName = GetFileName(dTime);
                    if (!File.Exists(fileName))
                        return "";
                    using (FileStream fileSteam = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    {
                        using (System.IO.StreamReader str = new StreamReader(fileSteam, Encoding.Unicode))
                        {
                            strret = str.ReadToEnd();
                            fileSteam.Close();
                        }
                    }
                    return strret;
                }
                catch
                {
                    return strret;
                }

            }
            #endregion
        }
    }

}
