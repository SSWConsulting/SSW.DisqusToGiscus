namespace DisqusToGiscusMigrator.Models;

public class Author(string fullName, string username)
{
    public string FullName { get; set; } = fullName;
    public string Username { get; set; } = username;

    public string AuthorMarkdown => $"[{FullName}](https://disqus.com/by/{Username})";
    public string AuthorAnchor => $"<a href=\"https://disqus.com/by/{Username}/\">{FullName}</a>";
}