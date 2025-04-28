using generator;

namespace ProjCharGeneratorTest;

public class BigramGeneratorTests
{
    private const string TestBigramsFile = "test_bigrams.txt";
    private const string TestOutputFile = "test_output.txt";
    private const string TestAnalysisFile = "test_analysis.txt";

    [Fact]
    public void Constructor_LoadsBigramsCorrectly()
    {
        File.WriteAllText(TestBigramsFile, "1 аб 100\n2 бв 200\n3 вг 300");

        var generator = new BigramGenerator(TestBigramsFile, TestAnalysisFile);

        Assert.Equal(3, generator.GetBigrams().Count);
        Assert.Equal(600, generator.GetTotalFrequencySum());

        File.Delete(TestBigramsFile);
    }

    [Fact]
    public void GenerateText_ReturnsCorrectLength()
    {
        File.WriteAllText(TestBigramsFile, "1 аб 100\n2 бв 200\n3 вг 300");
        var generator = new BigramGenerator(TestBigramsFile, TestAnalysisFile, 500);

        string text = generator.GenerateText();

        Assert.Equal(1000, text.Length);

        File.Delete(TestBigramsFile);
    }

    [Fact]
    public void SaveAnalysisData_CreatesFileWithCorrectFormat()
    {
        File.WriteAllText(TestBigramsFile, "1 аб 100\n2 бв 200");
        var generator = new BigramGenerator(TestBigramsFile, TestAnalysisFile, 10);
        generator.GenerateAndSave(TestOutputFile);

        Assert.True(File.Exists(TestAnalysisFile));
        var lines = File.ReadAllLines(TestAnalysisFile);

        Assert.Equal(2, lines.Length);

        foreach (var line in lines)
        {
            var parts = line.Split(' ');

            Assert.Equal(3, parts.Length);

            Assert.Equal(2, parts[0].Length);

            Assert.True(double.TryParse(parts[1], out _));
            Assert.True(double.TryParse(parts[2], out _));
        }

        File.Delete(TestBigramsFile);
        File.Delete(TestOutputFile);
        File.Delete(TestAnalysisFile);
    }

    [Fact]
    public void GetRandomBigram_ReturnsValidBigram()
    {
        // Arrange
        File.WriteAllText(TestBigramsFile, "1 аб 100\n2 бв 200");
        var generator = new BigramGenerator(TestBigramsFile, TestAnalysisFile);
        var bigrams = generator.GetBigrams().Keys.ToList();

        string bigram = generator.GetRandomBigram();

        Assert.Contains(bigram, bigrams);

        File.Delete(TestBigramsFile);
    }
}

public class WordFrequencyGeneratorTests
{
    private const string TestWordsFile = "test_words.txt";
    private const string TestOutputFile = "test_output.txt";
    private const string TestAnalysisFile = "test_analysis.txt";

    [Fact]
    public void Constructor_LoadsFrequenciesCorrectly()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "1 слово 0.1 0.1 100\n2 тест 0.1 0.2 200");

        // Act
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Assert
        Assert.Equal(2, generator.GetWordFrequencies().Count);
        Assert.Equal(300, generator.GetTotalFrequency());

        // Cleanup
        File.Delete(TestWordsFile);
    }

    [Fact]
    public void GenerateText_ReturnsCorrectWordCount()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "1 слово 100 0.1 100\n2 тест 200 0.2 200");
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Act
        string text = generator.GenerateText(50);

        // Assert
        Assert.Equal(50, text.Split(' ').Length);

        // Cleanup
        File.Delete(TestWordsFile);
    }

    [Fact]
    public void SaveAnalysisData_CreatesFileWithCorrectData()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "1 слово 100 0.1 100\n2 тест 200 0.2 200");
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Act
        generator.GenerateAndSave(TestOutputFile, 100);

        // Assert
        Assert.True(File.Exists(TestAnalysisFile));
        var lines = File.ReadAllLines(TestAnalysisFile);

        // Проверяем количество строк (должно соответствовать количеству уникальных слов)
        Assert.Equal(2, lines.Length);

        foreach (var line in lines)
        {
            var parts = line.Split(' ');

            // Проверяем структуру строки (3 части)
            Assert.Equal(3, parts.Length);

            // Проверяем, что первая часть - это слово (не пустое)
            Assert.False(string.IsNullOrEmpty(parts[0]));

            // Проверяем, что частоты являются числами с плавающей точкой
            Assert.True(double.TryParse(parts[1], out double actualFreq));
            Assert.True(double.TryParse(parts[2], out double expectedFreq));

            // Проверяем, что частоты в допустимом диапазоне (0-1)
            Assert.InRange(actualFreq, 0, 1);
            Assert.InRange(expectedFreq, 0, 1);
        }

        // Cleanup
        File.Delete(TestWordsFile);
        File.Delete(TestOutputFile);
        File.Delete(TestAnalysisFile);
    }

    [Fact]
    public void GenerateText_ThrowsWhenNoWordsLoaded()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "");
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Act & Assert
        var result = generator.GenerateText(10);
        Assert.Equal(string.Empty, result);

        // Cleanup
        File.Delete(TestWordsFile);
    }

    [Fact]
    public void GenerateAndSave_CreatesOutputFile()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "1 слово 100 0.1 100\n2 тест 200 0.2 200");
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Act
        generator.GenerateAndSave(TestOutputFile, 10);

        // Assert
        Assert.True(File.Exists(TestOutputFile));

        // Cleanup
        File.Delete(TestWordsFile);
        File.Delete(TestOutputFile);
    }

    [Fact]
    public void LoadFrequencies_HandlesInvalidLinesGracefully()
    {
        // Arrange
        File.WriteAllText(TestWordsFile, "1 слово 100 0.1 100\ninvalid line\n2 тест 200 0.2 200");

        // Act
        var generator = new WordFrequencyGenerator(TestWordsFile, TestAnalysisFile);

        // Assert
        Assert.Equal(2, generator.GetWordFrequencies().Count);

        // Cleanup
        File.Delete(TestWordsFile);
    }
}

