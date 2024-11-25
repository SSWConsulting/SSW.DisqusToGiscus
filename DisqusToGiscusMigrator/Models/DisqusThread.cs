namespace DisqusToGiscusMigrator.Models;

public class DisqusThread
{
    public long Id { get; }

    public string Title { get; set; }

    public string Url { get; set; }

    public List<DisqusPost> Posts { get; }

    public DateTime CreatedAt { get; set; }

    public DisqusThread(long id)
    {
        Id = id;
        Title = string.Empty;
        Url = string.Empty;
        Posts = new List<DisqusPost>();
    }
}