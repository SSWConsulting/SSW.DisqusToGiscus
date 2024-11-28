using System.Text.Json;
using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Helpers;
using DisqusToGiscusMigrator.Models;

namespace DisqusToGiscusMigrator;

public class Program
{
    private static readonly HttpClient _httpClient = new();
    private static readonly GitHubHelper _githubHelper = new();

    public static async Task Main()
    {
        var disqusBlogPosts = XmlParser.Parse(StaticSettings.DisqusCommentsPath);

        await RuleHelper.SetMarkdownFileLocation(disqusBlogPosts);
        await RuleHelper.SetGuid(disqusBlogPosts);

        await _githubHelper.AssociateDiscussions(disqusBlogPosts);

        WriteToJson(disqusBlogPosts);

        Logger.Log("Migration is finished", LogLevel.Info);
    }

    private static void WriteToJson(List<DisqusBlogPost> threads)
    {
        Logger.LogMethod(nameof(WriteToJson));

        var json = JsonSerializer.Serialize(threads, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(@"C:\\Users\\baban\\Downloads\\disqus-comments.json", json);
    }
}