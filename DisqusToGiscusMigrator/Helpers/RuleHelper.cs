using DisqusToGiscusMigrator.Constants;
using System.Text.Json;
using DisqusToGiscusMigrator.Models;
using Markdig;
using Markdig.Syntax;
using Markdig.Extensions.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DisqusToGiscusMigrator.Helpers;

public class RuleMetadata
{
    public string Guid { get; set; } = string.Empty;
}

public static class RuleHelper
{
    private static readonly HttpClient _httpClient = new();

    public static async Task SetMarkdownFileLocation(List<DisqusThread> threads)
    {
        Logger.Log(nameof(SetMarkdownFileLocation));

        var response = await _httpClient.GetAsync(StaticSettings.RulesHistoryJsonUrl);
        var rulesHistory = new List<RulesHistory>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            rulesHistory = JsonSerializer.Deserialize<List<RulesHistory>>(content) ?? [];
        }

        foreach (var thread in threads)
        {
            var forcedFileLocation = string.Empty;
            StaticSettings.ForcedFileLocations.TryGetValue(thread.Url, out forcedFileLocation);

            if (!string.IsNullOrWhiteSpace(forcedFileLocation))
            {
                thread.Rule.File = forcedFileLocation;
            }
            else
            {
                var uri = new Uri(thread.Url);
                var lastPart = uri.Segments.Last().Trim('/');

                thread.Rule.File = rulesHistory
                    .Where(rh => rh.File.Contains(lastPart, StringComparison.OrdinalIgnoreCase))
                    .Select(rh => rh.File.ToLower())
                    .FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(thread.Rule.File))
            {
                Console.WriteLine($"Wasn't able to get rule file location for thread ({thread.Id})");
            }
        }
    }

    public static async Task SetGuid(List<DisqusThread> threads)
    {
        Logger.Log(nameof(SetGuid));

        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        foreach (var thread in threads)
        {
            var url = $"{StaticSettings.GitHubRepoRawPath}{thread.Rule.File}";

            string? ruleRawContent = string.Empty;

            try
            {
                ruleRawContent = await _httpClient.GetStringAsync(url);
            }
            catch
            {
                Logger.Log($"Failed to access this rule file: {thread.Rule.File}", LogLevel.Warning);
                Logger.Log($"Failed disqus thread rule URL: {thread.Url}", LogLevel.Warning);
            }
            

            var document = Markdown.Parse(ruleRawContent, pipeline);
            var frontMatter = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (frontMatter is null)
            {
                continue;
            }

            var yaml = frontMatter.Lines.ToString();

            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var metadata = deserializer.Deserialize<RuleMetadata>(yaml);

            thread.Rule.Guid = metadata.Guid;
        }
    }
}
