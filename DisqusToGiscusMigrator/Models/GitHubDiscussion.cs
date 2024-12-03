namespace DisqusToGiscusMigrator.Models;

public class GitHubDiscussion
{
    public string ID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int Number { get; set; }
}