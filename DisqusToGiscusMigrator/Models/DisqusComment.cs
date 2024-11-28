namespace DisqusToGiscusMigrator.Models;

public class DisqusComment(
    long id,
    long disqusBlogPostId,
    long parentId,
    string message,
    DateTime createdAt,
    string authorName,
    string authorUsername)
{
    public long Id { get; } = id;

    public long DisqusBlogPostId { get; set; } = disqusBlogPostId;

    public long? ParentId { get; set; } = parentId;

    public string Message { get; set; } = message;

    public DateTime CreatedAt { get; set; } = createdAt;

    public string AuthorName { get; set; } = authorName;

    public string AuthorUsername { get; set; } = authorUsername;
}