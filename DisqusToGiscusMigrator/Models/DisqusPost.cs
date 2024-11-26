namespace DisqusToGiscusMigrator.Models;

public class DisqusPost
{
    public long Id { get; }
    public long ThreadId { get; set; }
    public long Parent { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
    public string Username { get; set; }

    public DisqusPost(long id)
    {
        Id = id;
        Message = string.Empty;
        Author = string.Empty;
        Username = string.Empty;
    }
}