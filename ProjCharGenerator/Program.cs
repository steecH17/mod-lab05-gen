using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Security.Principal;

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

    public class BigramGenerator
    {
        private readonly Dictionary<string, int> bigrams = new Dictionary<string, int>();
        private readonly Dictionary<string, int> generatedFrequency = new Dictionary<string, int>();
        private readonly Random random = new Random();
        private readonly string bigramsFilePath;
        private readonly string analysisFilePath;
        private readonly int textLength;
        private int totalFrequencySum;

        public BigramGenerator(string bigramsFilePath, string analysisFilePath, int textLength = 1000)
        {
            this.bigramsFilePath = bigramsFilePath;
            this.analysisFilePath = analysisFilePath;
            this.textLength = textLength;
            LoadBigrams();
        }

        public void GenerateAndSave(string resultFilePath)
        {
            string generatedText = GenerateText();
            File.WriteAllText(resultFilePath, generatedText);
            Console.WriteLine(generatedText.Length);
            SaveAnalysisData();
        }

        private void LoadBigrams()
        {
            if (!File.Exists(bigramsFilePath))
                throw new FileNotFoundException("Bigrams file not found", bigramsFilePath);

            foreach (var line in File.ReadAllLines(bigramsFilePath))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2) continue;

                string bigram = parts[1].Trim().ToLower();
                if (bigram.Length != 2) continue;

                if (int.TryParse(parts[2].Trim(), out int frequency))
                {
                    bigrams[bigram] = frequency;
                }
            }

            totalFrequencySum = bigrams.Values.Sum();
        }

        private string GenerateText()
        {
            if (bigrams.Count == 0)
                throw new InvalidOperationException("No bigrams loaded for generation");

            var result = new StringBuilder();

            for (int i = 0; i < textLength; i++)
            {
                string nextBigram = GetRandomBigram();
                UpdateFrequency(nextBigram);
                result.Append(nextBigram);
            }

            return result.ToString();
        }

        private string GetRandomBigram()
        {
            int randomValue = random.Next(totalFrequencySum);
            int cumulativeSum = 0;

            foreach (var pair in bigrams)
            {
                cumulativeSum += pair.Value;
                if (randomValue < cumulativeSum)
                    return pair.Key;
            }

            return bigrams.Keys.First();
        }

        private void UpdateFrequency(string bigram)
        {
            if (generatedFrequency.ContainsKey(bigram))
                generatedFrequency[bigram]++;
            else
                generatedFrequency[bigram] = 1;
        }

        private void SaveAnalysisData()
        {
            using (var writer = new StreamWriter(analysisFilePath, false, Encoding.UTF8))
            {
                foreach (var pair in generatedFrequency)
                {
                    double generatedFreq = (double)pair.Value / textLength;
                    double expectedFreq = (double)bigrams[pair.Key] / totalFrequencySum;

                    writer.WriteLine($"{pair.Key} {generatedFreq:F5} {expectedFreq:F5}");
                }
            }
        }
    }

    class WordFrequencyGenerator
    {
        private Dictionary<string, int> wordFrequencies = new Dictionary<string, int>();
        private List<string> words = new List<string>();
        private List<int> cumulativeFrequencies = new List<int>();
        private Random random = new Random();
        private int totalFrequency = 0;
        private readonly string wordsFilePath;
        private readonly string analysisFilePath;

        public WordFrequencyGenerator(string wordsPath, string analysisPath)
        {
            this.wordsFilePath = wordsPath;
            this.analysisFilePath = analysisPath;
            LoadFrequencies();
        }

        public void GenerateAndSave(string outputFile, int wordCount = 1000)
        {
            string generatedText = GenerateText(wordCount);
            File.WriteAllText(outputFile, generatedText, Encoding.UTF8);
            SaveAnalysisData(generatedText);
        }

        private void LoadFrequencies()
        {
            var lines = File.ReadAllLines(wordsFilePath, Encoding.UTF8);

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

        private string GenerateText(int wordCount)
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

        private void SaveAnalysisData(string generatedText)
        {
            var generatedWords = generatedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int totalGeneratedWords = generatedWords.Length;

            var generatedFrequencies = generatedWords
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

            using (var writer = new StreamWriter(analysisFilePath, false, Encoding.UTF8))
            {
                foreach (var word in words)
                {
                    // Получаем ожидаемую и реальную частоту
                    double expectedFrequency = (double)wordFrequencies[word] / totalFrequency;
                    generatedFrequencies.TryGetValue(word, out int actualCount);
                    double actualFrequency = (double)actualCount / totalGeneratedWords;

                    writer.WriteLine($"{word} {actualFrequency:F5} {expectedFrequency:F5}");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string bigramsPath = Path.Combine(Environment.CurrentDirectory, "InputData/bigrams_data.txt");
            string wordsPath = Path.Combine(Environment.CurrentDirectory, "InputData/words_data.txt");

            string bigramsOutputPath = Path.Combine("../Results", "gen-1.txt");
            string wordsOutputPath = Path.Combine("../Results", "gen-2.txt");

            string bigramsAnalysistPath = Path.Combine("../Results", "gen-1-analysis.txt");
            string wordsAnalysisPath = Path.Combine("../Results", "gen-2-analysis.txt");

            var bigramGenerator = new BigramGenerator(bigramsPath, bigramsAnalysistPath);
            bigramGenerator.GenerateAndSave(bigramsOutputPath);

            var wordGenerator = new WordFrequencyGenerator(wordsPath, wordsAnalysisPath);
            wordGenerator.GenerateAndSave(wordsOutputPath);
            // string bigramText = bigramGenerator.GenerateText(1000);
            // File.WriteAllText(bigramsOutputPath, bigramText);
            // Console.WriteLine($"Bigram text generated ({bigramText.Length} characters) and saved to Result/gen-1.txt");

            // Статистика по сгенерированному тексту
            // Console.WriteLine("\nSample of generated text (first 100 chars):");
            // Console.WriteLine(bigramText.Substring(0, Math.Min(100, bigramText.Length)));

        }
    }
}

