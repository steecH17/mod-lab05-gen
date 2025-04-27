using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace generator
{
    class CharGenerator 
    {
        private string syms = "абвгдеёжзийклмнопрстуфхцчшщьыъэюя"; 
        private char[] data;
        private int size;
        private Random random = new Random();
        public CharGenerator() 
        {
           size = syms.Length;
           data = syms.ToCharArray(); 
        }
        public char getSym() 
        {
           return data[random.Next(0, size)]; 
        }
    }

    class BigramGenerator
    {
        private Dictionary<string, Dictionary<string, int>> bigrams = new Dictionary<string, Dictionary<string, int>>();
        private List<string> words = new List<string>();
        private Random random = new Random();

        public BigramGenerator(string filePath)
        {
            DataReader(filePath);
        }

        public void DataReader(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    // Биграмма состоит из двух иероглифов подряд (2-й и 3-й элементы)
                    string bigram = parts[1] + parts[2];
                    int frequency = int.Parse(parts[3]); // Частота - 4-й элемент

                    // Разделяем биграмму на первое и второе слово
                    string firstWord = bigram.Substring(0, 1);
                    string secondWord = bigram.Substring(1, 1);

                    if (!bigrams.ContainsKey(firstWord))
                    {
                        bigrams[firstWord] = new Dictionary<string, int>();
                    }
                    bigrams[firstWord][secondWord] = frequency;
                    
                    // Добавляем слова в список для старта генерации
                    if (!words.Contains(firstWord))
                        words.Add(firstWord);
                }
            }
        }

        public string GenerateText(int length)
        {
            if (words.Count == 0 || bigrams.Count == 0)
                return string.Empty;

            List<string> result = new List<string>();
            
            // Начинаем со случайного слова
            string currentWord = words[random.Next(words.Count)];
            result.Add(currentWord);

            for (int i = 1; i < length; i++)
            {
                if (!bigrams.ContainsKey(currentWord) || bigrams[currentWord].Count == 0)
                {
                    // Если нет продолжения для текущего слова, выбираем случайное
                    currentWord = words[random.Next(words.Count)];
                }
                else
                {
                    // Выбираем следующее слово на основе частот биграмм
                    Dictionary<string, int> nextWords = bigrams[currentWord];
                    int total = nextWords.Values.Sum();
                    int randomValue = random.Next(total);
                    int cumulative = 0;

                    foreach (var pair in nextWords)
                    {
                        cumulative += pair.Value;
                        if (randomValue < cumulative)
                        {
                            currentWord = pair.Key;
                            break;
                        }
                    }
                }
                result.Add(currentWord);
            }

            return string.Join("", result); // Объединяем без пробелов для китайских иероглифов
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // CharGenerator gen = new CharGenerator();
            // SortedDictionary<char, int> stat = new SortedDictionary<char, int>();
            // for(int i = 0; i < 1000; i++) 
            // {
            //    char ch = gen.getSym(); 
            //    if (stat.ContainsKey(ch))
            //       stat[ch]++;
            //    else
            //       stat.Add(ch, 1); Console.Write(ch);
            // }
            // Console.Write('\n');
            // foreach (KeyValuePair<char, int> entry in stat) 
            // {
            //      Console.WriteLine("{0} - {1}",entry.Key,entry.Value/1000.0); 
            // }
             // Генерация текста на основе биграмм
            string bigramsPath = Path.Combine(Environment.CurrentDirectory, "bigrams_data.txt");
            string bigramsOutputPath = Path.Combine("../Results", "gen-1.txt");

            var bigramGenerator = new BigramGenerator(bigramsPath);
            string bigramText = bigramGenerator.GenerateText(1000);
            File.WriteAllText(bigramsOutputPath, bigramText);
            Console.WriteLine($"Bigram text generated ({bigramText.Length} characters) and saved to Result/gen-1.txt");
            
            // Статистика по сгенерированному тексту
            Console.WriteLine("\nSample of generated text (first 100 chars):");
            Console.WriteLine(bigramText.Substring(0, Math.Min(100, bigramText.Length)));
            
        }
    }
}

