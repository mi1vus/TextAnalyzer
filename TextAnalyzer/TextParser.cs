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
        public static int TotalWordsCount { get; set; }
        public static void ParseText(string path, bool append)
        {
            if (!File.Exists(path))
                return;

            //таймер учета быстродействия
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Dictionary<string, int> WordsToUpdate = new Dictionary<string, int>();
            Dictionary<string, int> WordsToAdd = new Dictionary<string, int>();

            var dbcontext = new DBHelper.WordsContext();
            var totalWords = dbcontext.Words.Count();
            if (totalWords > 0)
            {
                WordsToUpdate = dbcontext.Words.ToDictionary(k => k.Text, v => v.Count);
            }

            using (var reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().ToLower().Split(
                        new[] { " ", "-", ",", ".","?","!","\'","(",")","\"","0","1","2","3","4","5","6","7","8","9"}
                        ,StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in line)
                    {
                        ++TotalWordsCount;
                        if (!WordsToUpdate.ContainsKey(word))
                        {
                            //TODO ошибка с добавлением новых слов! надо обновлять 
                            WordsToAdd.Add(word, 1);
                        }
                        else
                        {
                            WordsToUpdate[word] = ++WordsToUpdate[word];
                        }
                    }
                }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Logger.Write($"чтение файла заняло: {elapsedTime}");

            stopWatch.Reset();
            stopWatch.Start();

            foreach (var word in WordsToAdd)
            {
                if (word.Key.Length > 2 && word.Key.Length <= 15)
                dbcontext.Words.Add(
                    new DBHelper.Word {
                        Text = word.Key,
                        Count = word.Value
                    }
                    );
            }

            stopWatch.Stop();
            ts = stopWatch.Elapsed;

            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Logger.Write($"добавление новых слов в контекст заняло: {elapsedTime}");

            stopWatch.Reset();
            stopWatch.Start();

            foreach (var word in WordsToUpdate)
            {
                dbcontext.Words.First(w => w.Text == word.Key).Count = word.Value;
            }

            stopWatch.Stop();
            ts = stopWatch.Elapsed;

            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Logger.Write($"обновление слов в контексте заняло: {elapsedTime}");

            stopWatch.Reset();
            stopWatch.Start();

            dbcontext.SaveChanges();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;

            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Logger.Write($"сохранение контексте заняло: {elapsedTime}");

            stopWatch.Reset();
            stopWatch.Start();


            ///TODO удалить после тестирования
            Dictionary<string, int> SWordsFrequency = dbcontext.Words.OrderByDescending(t => t.Count).ThenByDescending(t => t.Text).ToDictionary(k => k.Text, v => v.Count);
        }
    }
}
