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
        //public static int TotalWordsCount { get; set; }
        public static void ParseTextToDB(string path, bool append)
        {
            if (!File.Exists(path))
            {
                Logger.Write($"Отсутствует файл для анализа {path}");
                return;
            }

            Logger.Write($"Начало анализа {path}\r\n" +
                $"обновление существующего словаря - {append}");

            Dictionary<string, int> wordsToUpdate = new Dictionary<string, int>();
            Dictionary<string, int> wordsToAdd = new Dictionary<string, int>();
            var fileWordsCount = 0;

            Logger.StartTimer();

            var dbcontext = new DBHelper.WordsContext();
            var dbWordsCount = dbcontext.Words.Count();
            if (dbWordsCount > 0)
            {
                wordsToUpdate = dbcontext.Words.ToDictionary(k => k.Text, v => v.Count);
            }

            Logger.LogTimerAndRestart($"Чтение слов из БД [{dbWordsCount} слов] заняло:");

            using (var reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().ToLower().Split(
                        new[] { " ", "-", ",", ".","\"",
                            "?","!","\'","(",")","0","1",
                            "2","3","4","5","6","7","8","9"}
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

            Logger.LogTimerAndRestart($"чтение файла [{fileWordsCount} слов] заняло:");

            foreach (var word in wordsToAdd)
            {
                dbcontext.Words.Add(
                    new DBHelper.Word {
                        Text = word.Key,
                        Count = word.Value
                    }
                    );
            }

            Logger.LogTimerAndRestart($"добавление новых слов в контекст заняло:");

            foreach (var word in wordsToUpdate)
            {
                dbcontext.Words.First(w => w.Text == word.Key).Count = word.Value;
            }

            Logger.LogTimerAndRestart($"обновление слов в контексте заняло:");

            dbcontext.SaveChanges();

            Logger.LogTimerAndRestart($"сохранение контексте заняло:");

            ///TODO удалить после тестирования
            Dictionary<string, int> SWordsFrequency = dbcontext.Words.OrderByDescending(t => t.Count).ThenByDescending(t => t.Text).ToDictionary(k => k.Text, v => v.Count);
        }
    }
}
