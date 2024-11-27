namespace DisqusToGiscusMigrator.Models;

public class DisqusThread(long id)
{
    public long Id { get; } = id;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public List<DisqusPost> Posts { get; } = [];

    public DateTime CreatedAt { get; set; }

    public Rule Rule { get; set; } = new Rule();
}