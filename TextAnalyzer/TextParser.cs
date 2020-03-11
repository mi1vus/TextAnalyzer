using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity;

namespace TextAnalyzer
{
    public class TextParser
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("TextParser");
        private static DBHelper.WordsContext ModifyDbСontext;
        private static DBHelper.WordsContext QueryDbСontext;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="connectionString">Строка подключения к безе данных</param>
        /// <returns>False - если возникла ошибка</returns>
        public static bool Initialize(string connectionString)
        {
            try
            {
                ModifyDbСontext = new DBHelper.WordsContext(connectionString);
                QueryDbСontext = new DBHelper.WordsContext(connectionString);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write($"Ошибка в Initialize: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Анализ текста и добавление слов в базу данных
        /// </summary>
        /// <param name="path">Расположение анализируемого файла</param>
        /// <param name="updateDB">True - добавить слова в существующую таблицу, false - создать новую таблицу</param>
        /// <returns>False - если возникла ошибка</returns>
        public static bool ParseTextToDB(string path, bool updateDB)
        {
            if (!File.Exists(path))
            {
                Logger.Write($"Отсутствует файл для анализа {path}");
                return false;
            }
            if (ModifyDbСontext == null || QueryDbСontext == null)
            {
                Logger.Write("TextParser не инициализирован!");
                return false;
            }
            try
            {
                Logger.Write($"Начало анализа текста {path}{Environment.NewLine}" +
                    $"обновление существующего словаря - {updateDB}");

                Dictionary<string, int> wordsFromDb = new Dictionary<string, int>();
                Dictionary<string, int> wordsToUpdate = new Dictionary<string, int>();
                Dictionary<string, int> wordsToAdd = new Dictionary<string, int>();
                var fileWordsCount = 0;

                Logger.StartTimer();

                #region Анализ и подготовка базы данных

                var dbWordsCount = ModifyDbСontext.Words.Count();

                if (updateDB)
                {
                    if (dbWordsCount > 0)
                    {
                        wordsFromDb = ModifyDbСontext.Words.ToDictionary(k => k.Text, v => v.Count);
                    }
                }
                else
                {
                    ModifyDbСontext.Words.RemoveRange(ModifyDbСontext.Words);
                    lock (QueryDbСontext)
                        ModifyDbСontext.SaveChanges();
                }

                #endregion

                Logger.LogTimerAndRestart((updateDB ? "Чтение" : "Удаление") + $"слов из БД [{wordsFromDb.Count} слов] заняло:");

                #region Анализ файла
                
                using (var reader = new StreamReader(path))
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().ToLower().Split(
                            new[] { " ", "-", ",", ".","\"",":", ";",
                            "?","!","\'","(",")","(",")","[","]",
                            "0","1","2","3","4","5","6","7","8","9"}
                            , StringSplitOptions.RemoveEmptyEntries);

                        foreach (var word in line)
                        {
                            if (word.Length < 3 || word.Length > 15)
                                continue;

                            ++fileWordsCount;
                            //Слово еще не добавлено в базу данных
                            if (!wordsFromDb.ContainsKey(word))
                            {
                                if (!wordsToAdd.ContainsKey(word))
                                    wordsToAdd.Add(word, 1);
                                else
                                    ++wordsToAdd[word];
                            }
                            else
                            {
                                if (!wordsToUpdate.ContainsKey(word))
                                    wordsToUpdate.Add(word, wordsFromDb[word] + 1);
                                else
                                    ++wordsToUpdate[word];

                            }
                        }
                    }

                #endregion

                Logger.LogTimerAndRestart($"Чтение файла [{fileWordsCount} слов] заняло:");

                #region Добавление слов в БД

                ModifyDbСontext.Words.AddRange(wordsToAdd.Where(w => w.Value > 2).Select(w => new DBHelper.Word
                {
                    Text = w.Key,
                    Count = w.Value
                }));

                #endregion

                Logger.LogTimerAndRestart($"Добавление новых слов [{wordsToAdd.Where(w => w.Value > 2).Count()}] в контекст заняло:");

                #region Обновление слов в БД

                foreach (var word in wordsToUpdate.Where(w => w.Value > 2))
                {
                    ModifyDbСontext.Words.First(w => w.Text == word.Key).Count = word.Value;
                }

                #endregion

                Logger.LogTimerAndRestart($"Обновление слов [{wordsToUpdate.Where(w => w.Value > 2).Count()}] в контексте заняло:");

                lock (QueryDbСontext)
                    ModifyDbСontext.SaveChanges();

                Logger.LogTimerAndRestart($"Сохранение контекста заняло:");
                Logger.ResetTimer();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write($"Ошибка в ParseTextToDB: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Возвращает самые часто встречающиеся слова из словаря
        /// </summary>
        /// <param name="prefix">Начало слова</param>
        /// <param name="logTime">Логирование затраченного времени</param>
        /// <param name="top">Число выбираемых слов</param>
        /// <returns>Список подходящих слов в порядке уменьшения частоты встречаемости
        /// и алфавитном порядке</returns>
        public static List<String> GetNearWords(string prefix, bool logTime = true, int top = 5)
        {
            List<String> result = new List<string>();
            if (QueryDbСontext == null)
            {
                Logger.Write("TextParser не инициализирован!");
                return result;
            }
            try
            {
                Logger.StartTimer();
                IOrderedQueryable<DBHelper.Word> query = 
                    QueryDbСontext.Words.Where(w => w.Text.StartsWith(prefix))
                        .OrderByDescending(w => w.Count).ThenBy(w => w.Text);

                if (top > 0)
                    query = (IOrderedQueryable<DBHelper.Word>)query.Take(top);

                lock (QueryDbСontext)
                    result = query.Select(w => w.Text).ToList();

                if (logTime)
                    Logger.LogTimerAndRestart($"Запрос автодополнения к \"{prefix}\". Найдено {result.Count} слов. Запрос занял:");
                Logger.ResetTimer();
            }
            catch (Exception ex)
            {
                Logger.Write($"Ошибка в GetNearWords: {ex.ToString()}");
            }
            return result;
        }

        /// <summary>
        /// Очистка базы данных со словами
        /// </summary>
        /// <returns>False - если возникла ошибка</returns>
        public static bool ClearDB()
        {
            if (ModifyDbСontext == null)
            {
                Logger.Write("TextParser не инициализирован!");
                return false;
            }
            try
            {
                ModifyDbСontext.Words.RemoveRange(ModifyDbСontext.Words);
                lock (QueryDbСontext)
                    ModifyDbСontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write($"Ошибка в ClearDB: {ex.ToString()}");
                return false;
            }
        }
    }
}
