using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics;

namespace TextAnalyzer
{
    public class TextParser
    {
        private static ProjectSummer.Logger Logger = new ProjectSummer.Logger("TextParser");
        public static DBHelper.WordsContext DbСontext = new DBHelper.WordsContext("Data Source=.\\SQLEXP; Initial Catalog=TextAnalyzer; Integrated Security=true; MultipleActiveResultSets=true;");
        //public static int TotalWordsCount { get; set; }
        public static void ParseTextToDB(string path, bool update)
        {
            if (!File.Exists(path))
            {
                Logger.Write($"Отсутствует файл для анализа {path}");
                return;
            }

            Logger.Write($"Начало анализа {path}{Environment.NewLine}" +
                $"обновление существующего словаря - {update}");

            Dictionary<string, int> wordsFromDb = new Dictionary<string, int>();
            Dictionary<string, int> wordsToUpdate = new Dictionary<string, int>();
            Dictionary<string, int> wordsToAdd = new Dictionary<string, int>();
            var fileWordsCount = 0;

            Logger.StartTimer();

            if (update)
            {
                var dbWordsCount = DbСontext.Words.Count();
                if (dbWordsCount > 0)
                {
                    wordsFromDb = DbСontext.Words.ToDictionary(k => k.Text, v => v.Count);
                }
            }
            else
            {
                DbСontext.Words.RemoveRange(DbСontext.Words);
                DbСontext.SaveChanges();
            }

            Logger.LogTimerAndRestart($"Чтение слов из БД [{wordsFromDb.Count} слов] заняло:");

            using (var reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().ToLower().Split(
                        new[] { " ", "-", ",", ".","\"",
                            "?","!","\'","(",")","(",")","[","]",
                            "0","1","2","3","4","5","6","7","8","9"}
                        ,StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in line)
                    {
//TODO убрать
#if DEBUG
#else
#endif
                        if (word.Length < 3 || word.Length > 15)
                            continue;

                        ++fileWordsCount;
                        if (!wordsFromDb.ContainsKey(word))
                        {
                            //TODO ошибка с добавлением новых слов! надо обновлять 
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

            Logger.LogTimerAndRestart($"Чтение файла [{fileWordsCount} слов] заняло:");

            DbСontext.Words.AddRange(wordsToAdd.Where(w=>w.Value > 2).Select(w=> new DBHelper.Word
            {
                Text = w.Key,
                Count = w.Value
            }));
            //foreach (var word in wordsToAdd)
            //{
            //    //TODO убрать
            //    if (word.Key.Length < 3 || word.Key.Length > 15)
            //        continue;

            //    DbСontext.Words.AddRange(
            //    new DBHelper.Word {
            //            Text = word.Key,
            //            Count = word.Value
            //        }
            //        );
            //}

            Logger.LogTimerAndRestart($"Добавление новых слов [{wordsToAdd.Where(w => w.Value > 2).Count()}] в контекст заняло:");

            foreach (var word in wordsToUpdate.Where(w => w.Value > 2))
            {
                DbСontext.Words.First(w => w.Text == word.Key).Count = word.Value;
            }

            Logger.LogTimerAndRestart($"Обновление слов [{wordsToUpdate.Where(w => w.Value > 2).Count()}] в контексте заняло:");

            DbСontext.SaveChanges();

            Logger.LogTimerAndRestart($"Сохранение контекста заняло:");
            Logger.ResetTimer();
            ///TODO удалить после тестирования
            Dictionary<string, int> SWordsFrequency = DbСontext.Words.OrderByDescending(t => t.Count).ThenByDescending(t => t.Text).ToDictionary(k => k.Text, v => v.Count);
        }

        public static List<String> GetNearWords(string prefix, bool logTime = true)
        {
            Logger.StartTimer();
            var result = DbСontext.Words.Where(w => w.Text.StartsWith(prefix))
                .OrderByDescending(w => w.Count)
                .ThenBy(w => w.Text)
                .Take(5).Select(w => w.Text).ToList();
            if (logTime)
                Logger.LogTimerAndRestart($"Запрос автодополнения к \"{prefix}\". Найдено {result.Count} слов. Запрос занял:");
            Logger.ResetTimer();
            return result;
        }

        public static void ClearDB()
        {
            DbСontext.Words.RemoveRange(DbСontext.Words);
            DbСontext.SaveChanges();
        }
        //TODO удалить
        //public static List<String> GetNearWordsNoOrder(string prefix)
        //{
        //    Logger.StartTimer();
        //    var result = DbСontext.Words.Where(w => w.Text.StartsWith(prefix))
        //        .Take(5).Select(w => w.Text).ToList();
        //    Logger.LogTimerAndRestart($"Запрос без выравн автодополнения к \"{prefix}\". Найдено {result.Count} слов. Запрос занял:");
        //    Logger.ResetTimer();
        //    return result;
        //}
    }
}
