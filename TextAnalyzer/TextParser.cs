using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    //public class Word {
    //    public int Count { get; set; }
    //}

    public class TextParser
    {
        public static int TotalWordsCount { get; set; }
        public static Dictionary<string, int> WordsFrequency = new Dictionary<string, int>();
        public static void ParseText(string path, bool append)
        {
            if (!File.Exists(path))
                return;
            
            using (var reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().ToLower().Split(
                        new[] { " ", ",", ".","?","!","\'","(",")","\"","0","1","2","3","4","5","6","7","8","9"}
                        ,StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in line)
                    {
                        ++TotalWordsCount;
                        if (!WordsFrequency.ContainsKey(word))
                            WordsFrequency.Add(word, 1);
                        else
                            WordsFrequency[word] = ++WordsFrequency[word];
                    }
                }
            Dictionary<string, int> SWordsFrequency = new Dictionary<string, int>();
            foreach (var pair in WordsFrequency.OrderByDescending(t => t.Value))
                SWordsFrequency.Add(pair.Key, pair.Value);
        }
    }
}
