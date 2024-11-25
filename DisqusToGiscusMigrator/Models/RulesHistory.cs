using System.Text.Json.Serialization;

namespace DisqusToGiscusMigrator.Models;

public class RulesHistory
{
    [JsonPropertyName("file")]
    public string File { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}
