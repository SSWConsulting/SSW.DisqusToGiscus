using Octokit.GraphQL;

namespace DisqusToGiscusMigrator.Models;

public class GitHubDiscussion
{
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public ID ID { get; set; }

    public int Number { get; set; }
}