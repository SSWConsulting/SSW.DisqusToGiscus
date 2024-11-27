namespace DisqusToGiscusMigrator.Models;

public class DisqusPost(long id)
{
    public long Id { get; } = id;

    public long ThreadId { get; set; }

    public long Parent { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Author { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}