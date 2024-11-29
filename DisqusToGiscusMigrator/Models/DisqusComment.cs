namespace DisqusToGiscusMigrator.Models;

public class DisqusComment(
    long id,
    long disqusBlogPostId,
    long? parentId,
    string message,
    DateTime createdAt,
    string authorName,
    string authorUsername)
{
    public long Id { get; } = id;

    public long DisqusBlogPostId { get; } = disqusBlogPostId;

    public long? ParentId { get; set; } = parentId;

    public string Message { get; set; } = message;

    public DateTime CreatedAt { get; } = createdAt;

    public string AuthorName { get; } = authorName;

    public string AuthorUsername { get; } = authorUsername;

    public GitHubComment? AssociatedGitHubComment { get; set; }
}