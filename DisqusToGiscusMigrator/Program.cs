using System.Text.Json;
using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Helpers;
using DisqusToGiscusMigrator.Models;

namespace DisqusToGiscusMigrator;

public class Program
{
    private static readonly HttpClient _httpClient = new();

    public static async Task Main()
    {
        var path = StaticSettings.DisqusCommentsPath;
        var threads = XmlParser.Parse(path);

        await RuleHelper.SetMarkdownFileLocation(threads);
        await RuleHelper.SetGuid(threads);

        WriteToJson(threads);

        Logger.Log("Migration is finished", LogLevel.Info);
    }

    private static void WriteToJson(List<DisqusThread> threads)
    {
        Logger.Log(nameof(WriteToJson));

        var json = JsonSerializer.Serialize(threads, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(@"C:\\Users\\baban\\Downloads\\disqus-comments.json", json);
    }
}