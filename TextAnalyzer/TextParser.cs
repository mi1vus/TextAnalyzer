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
        public static DBHelper.WordsContext DbСontext = new DBHelper.WordsContext();
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

            Dictionary<string, int> wordsToUpdate = new Dictionary<string, int>();
            Dictionary<string, int> wordsToAdd = new Dictionary<string, int>();
            var fileWordsCount = 0;

            Logger.StartTimer();

            if (update)
            {
                var dbWordsCount = DbСontext.Words.Count();
                if (dbWordsCount > 0)
                {
                    wordsToUpdate = DbСontext.Words.ToDictionary(k => k.Text, v => v.Count);
                }
            }
            else
            {
                DbСontext.Words.RemoveRange(DbСontext.Words);
                DbСontext.SaveChanges();
            }

            Logger.LogTimerAndRestart($"Чтение слов из БД [{wordsToUpdate.Count} слов] заняло:");

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
                        if (word.Length < 3 || word.Length > 15)
                            continue;
#endif
                        ++fileWordsCount;
                        if (!wordsToUpdate.ContainsKey(word))
                        {
                            //TODO ошибка с добавлением новых слов! надо обновлять 
                            if (!wordsToAdd.ContainsKey(word))
                                wordsToAdd.Add(word, 1);
                            else
                                ++wordsToAdd[word];
                        }
                        else
                        {
                            ++wordsToUpdate[word];
                        }
                    }
                }

            Logger.LogTimerAndRestart($"Чтение файла [{fileWordsCount} слов] заняло:");

            foreach (var word in wordsToAdd)
            {
                //TODO убрать
                if (word.Key.Length < 3 || word.Key.Length > 15)
                    continue;

                DbСontext.Words.Add(
                new DBHelper.Word {
                        Text = word.Key,
                        Count = word.Value
                    }
                    );
            }

            Logger.LogTimerAndRestart($"Добавление новых слов в контекст заняло:");

            foreach (var word in wordsToUpdate)
            {
                DbСontext.Words.First(w => w.Text == word.Key).Count = word.Value;
            }

            Logger.LogTimerAndRestart($"Обновление слов в контексте заняло:");

            DbСontext.SaveChanges();

            Logger.LogTimerAndRestart($"Сохранение контексте заняло:");
            Logger.ResetTimer();
            ///TODO удалить после тестирования
            Dictionary<string, int> SWordsFrequency = DbСontext.Words.OrderByDescending(t => t.Count).ThenByDescending(t => t.Text).ToDictionary(k => k.Text, v => v.Count);
        }

        public static List<String> GetNearWords(string prefix)
        {
            Logger.StartTimer();
            var result = DbСontext.Words.Where(w => w.Text.StartsWith(prefix))
                .OrderByDescending(w => w.Count)
                //.ThenBy(w => w.Text)
                .Take(5).Select(w => w.Text).ToList();
            Logger.LogTimerAndRestart($"Запрос автодополнения к \"{prefix}\". Найдено {result.Count} слов. Запрос занял:");
            Logger.ResetTimer();
            return result;
        }
        //TODO удалить
        public static List<String> GetNearWordsNoOrder(string prefix)
        {
            Logger.StartTimer();
            var result = DbСontext.Words.Where(w => w.Text.StartsWith(prefix))
                .Take(5).Select(w => w.Text).ToList();
            Logger.LogTimerAndRestart($"Запрос без выравн автодополнения к \"{prefix}\". Найдено {result.Count} слов. Запрос занял:");
            Logger.ResetTimer();
            return result;
        }
    }
}
