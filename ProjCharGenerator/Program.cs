using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

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

        private List<string> startingWords = new List<string>();
        private Random random = new Random();

        public BigramGenerator(string filePath)
        {
            DataReader(filePath);
        }

        public void DataReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Bigram data file not found", filePath);

            foreach (string line in File.ReadLines(filePath))
            {
                // Разбиваем строку на компоненты (разделители: пробелы и табы)
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // Проверяем, что строка содержит минимум 3 элемента (номер, биграмма, частота)
                if (parts.Length >= 3)
                {
                    // Биграмма - это второй элемент (индекс 1)
                    string bigram = parts[1];

                    // Разделяем биграмму на два слова
                    if (bigram.Length >= 2)
                    {
                        string firstWord = bigram.Substring(0, 1);
                        string secondWord = bigram.Substring(1, 1);
                        int frequency = int.Parse(parts[2]);

                        // Добавляем в словарь биграмм
                        if (!bigrams.ContainsKey(firstWord))
                            bigrams[firstWord] = new Dictionary<string, int>();

                        bigrams[firstWord][secondWord] = frequency;

                        // Добавляем слово в список стартовых слов
                        if (!startingWords.Contains(firstWord))
                            startingWords.Add(firstWord);
                    }
                }
            }
        }

        public string GenerateText(int wordCount)
        {
            if (startingWords.Count == 0)
                return "No bigram data available for text generation";

            List<string> result = new List<string>();

            // Выбираем случайное стартовое слово
            string currentWord = startingWords[random.Next(startingWords.Count)];
            result.Add(currentWord);

            for (int i = 1; i < wordCount; i++)
            {
                // Если для текущего слова нет продолжений, выбираем случайное слово
                if (!bigrams.ContainsKey(currentWord) || bigrams[currentWord].Count == 0)
                {
                    currentWord = startingWords[random.Next(startingWords.Count)];
                }
                else
                {
                    // Выбираем следующее слово на основе частот
                    Dictionary<string, int> nextWords = bigrams[currentWord];
                    int totalFrequency = nextWords.Values.Sum();
                    int randomValue = random.Next(totalFrequency);
                    int cumulativeFrequency = 0;

                    foreach (var pair in nextWords)
                    {
                        cumulativeFrequency += pair.Value;
                        if (randomValue < cumulativeFrequency)
                        {
                            currentWord = pair.Key;
                            break;
                        }
                    }
                }
                result.Add(currentWord);
            }

            return string.Join("", result); // Объединяем без пробелов
        }

        public void GenerateOutput(string outputFile, int wordCount)
        {
            string generatedText = GenerateText(wordCount);
            File.WriteAllText(outputFile, generatedText, Encoding.UTF8);
        }
    }

    class WordFrequencyGenerator
    {
        private Dictionary<string, int> wordFrequencies = new Dictionary<string, int>();
        private List<string> words = new List<string>();
        private List<int> cumulativeFrequencies = new List<int>();
        private Random random = new Random();
        private int totalFrequency = 0;

        public WordFrequencyGenerator(string filePath)
        {
            LoadFrequencies(filePath);
        }

        public void LoadFrequencies(string filePath)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Console.WriteLine("File OPen");
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double frequency))
                {
                    string word = parts[1].Trim();
                    wordFrequencies[word] = (int)frequency;
                    words.Add(word);
                    totalFrequency += (int)frequency;
                    cumulativeFrequencies.Add(totalFrequency);
                }
            }
        }

        public string GenerateText(int wordCount)
        {
            if (words.Count == 0) return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < wordCount; i++)
            {
                int randomValue = random.Next(totalFrequency);
                int wordIndex = cumulativeFrequencies.FindIndex(x => x > randomValue);
                result.Append(words[wordIndex] + " ");
            }
            return result.ToString().Trim();
        }

        public void GenerateOutput(string outputFile, int wordCount)
        {
            string generateText = GenerateText(wordCount);
            File.WriteAllText(outputFile, generateText, Encoding.UTF8);
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
            string wordsPath = Path.Combine(Environment.CurrentDirectory, "words_data.txt");
            string bigramsOutputPath = Path.Combine("../Results", "gen-1.txt");
            string wordsOutputPath = Path.Combine("../Results", "gen-2.txt");

            var bigramGenerator = new BigramGenerator(bigramsPath);
            bigramGenerator.GenerateOutput(bigramsOutputPath, 1000);

            var wordGenerator = new WordFrequencyGenerator(wordsPath);
            wordGenerator.GenerateOutput(wordsOutputPath, 1000);
            // string bigramText = bigramGenerator.GenerateText(1000);
            // File.WriteAllText(bigramsOutputPath, bigramText);
            // Console.WriteLine($"Bigram text generated ({bigramText.Length} characters) and saved to Result/gen-1.txt");

            // Статистика по сгенерированному тексту
            // Console.WriteLine("\nSample of generated text (first 100 chars):");
            // Console.WriteLine(bigramText.Substring(0, Math.Min(100, bigramText.Length)));

        }
    }
}

