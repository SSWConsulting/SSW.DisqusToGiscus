using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Models;
using System.Text.Json;
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

    public static async Task SetMarkdownFileLocation(List<DisqusBlogPost> blogPosts)
    {
        Logger.LogMethod(nameof(SetMarkdownFileLocation));

        var response = await _httpClient.GetAsync(StaticSettings.RulesHistoryJsonUrl);
        var rulesHistory = new List<RulesHistory>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            rulesHistory = JsonSerializer.Deserialize<List<RulesHistory>>(content) ?? [];
        }

        foreach (var post in blogPosts)
        {
            var forcedFileLocation = string.Empty;
            StaticSettings.ForcedFileLocations.TryGetValue(post.Url, out forcedFileLocation);

            if (!string.IsNullOrWhiteSpace(forcedFileLocation))
            {
                post.Rule.File = forcedFileLocation;
            }
            else
            {
                var uri = new Uri(post.Url);
                var lastPart = uri.Segments.Last().Trim('/');

                post.Rule.File = rulesHistory
                    .Where(rh => rh.File.Contains(lastPart, StringComparison.OrdinalIgnoreCase))
                    .Select(rh => rh.File.ToLower())
                    .FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(post.Rule.File))
            {
                Logger.Log($"Rule file location is empty for Disqus blog post: {post.Id}", LogLevel.Error);
                throw new Exception();
            }
        }
    }

    public static async Task SetGuid(List<DisqusBlogPost> blogPosts)
    {
        Logger.LogMethod(nameof(SetGuid));

        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        foreach (var post in blogPosts)
        {
            var ruleRawContent = string.Empty;
            var url = $"{StaticSettings.ContentRepoRawPath}{post.Rule.File}";

            try
            {
                ruleRawContent = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to access rule file: {post.Rule.File}", LogLevel.Error);
                throw new Exception(ex.Message);
            }
            
            if (string.IsNullOrWhiteSpace(ruleRawContent))
            {
                Logger.Log($"Rule raw content is empty rule file: {post.Rule.File}", LogLevel.Error);
                throw new Exception();
            }

            var document = Markdown.Parse(ruleRawContent, pipeline);
            var frontMatter = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (frontMatter is null)
            {
                Logger.Log($"Frontmatter is null for rule file: {post.Rule.File}", LogLevel.Error);
                throw new Exception();
            }

            var yaml = frontMatter.Lines.ToString();
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            try
            {
                var metadata = deserializer.Deserialize<RuleMetadata>(yaml);
                post.Rule.Guid = metadata.Guid;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to deserialize YAML for rule file: {post.Rule.File}", LogLevel.Error);
                throw new Exception(ex.Message);
            }
        }
    }
}
