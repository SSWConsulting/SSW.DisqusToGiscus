namespace DisqusToGiscusMigrator.Models;

public class DisqusBlogPost(long id, string title, string url, DateTime createdAt)
{
    public long Id { get; } = id;

    public string Title { get; set; } = title;

    public string Url { get; set; } = url;

    public List<DisqusComment> DisqusComments { get; set; } = new();

    public DateTime CreatedAt { get; set; } = createdAt;

    public Rule Rule { get; set; } = new Rule();

    public GitHubDiscussion? AssociatedGitHubDiscussion { get; set; }
}