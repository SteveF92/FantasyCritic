using System.Globalization;
using CsvHelper;
using FantasyCritic.SharedSerialization.Database;
using FantasyCritic.Test.TestUtilities;

namespace FantasyCritic.TestDataScrubber;

internal class Program
{
    private const string _basePath = "..\\..\\..\\..\\FantasyCritic.Test\\TestData\\PostSummerGamesFest2022";
    private static readonly string _resultsPath = Path.Combine("..\\..\\..\\", "Results");
    static void Main()
    {
        var leagueYears = GetLeagueYearEntities();
        var leaguePairs = leagueYears.Select(x => new GuidMap(x.LeagueID, Guid.NewGuid())).ToList();

        var publishers = GetPublisherEntities();
        var publisherPairs = publishers.Select(x => new GuidMap(x.PublisherID, Guid.NewGuid())).ToList();

        WriteMapFile("LeaguesMap.csv", leaguePairs);
        WriteMapFile("PublishersMap.csv", publisherPairs);

        var files = Directory.GetFiles(_basePath);
        for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            var file = files[fileIndex];
            var fileName = Path.GetFileName(file);
            Console.WriteLine($"Scrubbing file: {fileName} ({fileIndex + 1}/{files.Length})");
            string text = File.ReadAllText(file);
            for (var leagueIndex = 0; leagueIndex < leaguePairs.Count; leagueIndex++)
            {
                var league = leaguePairs[leagueIndex];
                Console.Write($"\r Leagues: {leagueIndex + 1}/{leaguePairs.Count}");
                text = text.Replace(league.Actual.ToString(), league.Scrubbed.ToString());
            }
            Console.WriteLine();

            for (var publisherIndex = 0; publisherIndex < publisherPairs.Count; publisherIndex++)
            {
                var publisher = publisherPairs[publisherIndex];
                Console.Write($"\r Publishers: {publisherIndex + 1}/{publisherPairs.Count}");
                text = text.Replace(publisher.Actual.ToString(), publisher.Scrubbed.ToString());
            }
            Console.WriteLine();

            var newFile = Path.Combine(_resultsPath, fileName);
            File.WriteAllText(newFile, text);
        }
    }

    private static IReadOnlyList<LeagueYearEntity> GetLeagueYearEntities()
    {
        var path = Path.Combine(_basePath, "LeagueYears.csv");
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<LeagueYearEntityMap>();
        var leagueYearEntities = csv.GetRecords<LeagueYearEntity>().ToList();
        return leagueYearEntities;
    }

    private static IReadOnlyList<TestPublisherEntity> GetPublisherEntities()
    {
        using var reader = new StreamReader(Path.Combine(_basePath, "Publishers.csv"));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var publishers = csv.GetRecords<TestPublisherEntity>().ToList();
        return publishers;
    }

    private static void WriteMapFile(string fileName, IEnumerable<GuidMap> map)
    {
        Directory.CreateDirectory(_resultsPath);
        var mapsPath = Path.Combine(_resultsPath, "Maps");
        Directory.CreateDirectory(mapsPath);
        using var writer = new StreamWriter(Path.Combine(mapsPath, fileName));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(map);
    }
}
