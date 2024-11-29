namespace DisqusToGiscusMigrator.Models;

public class DisqusComment(
    long id,
    long disqusBlogPostId,
    long parentId,
    string message,
    DateTime createdAt,
    Author author)
{
    public long Id { get; } = id;

    public long DisqusBlogPostId { get; } = disqusBlogPostId;

    public long ParentId { get; set; } = parentId;

    public string Message { get; set; } = message;

    public DateTime CreatedAt { get; } = createdAt;

    public Author Author { get; } = author;

    public List<DisqusComment> ChildComments { get; set; } = new();

    public GitHubComment? AssociatedGitHubComment { get; set; }
}