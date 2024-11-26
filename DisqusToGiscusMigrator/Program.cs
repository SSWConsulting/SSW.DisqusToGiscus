using System.Text.Json;
using System.Web;
using System.Xml;
using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Helpers;
using DisqusToGiscusMigrator.Models;

namespace DisqusToGiscusMigrator;

public class Program
{
    private static readonly HttpClient _httpClient = new();

    public static async Task Main()
    {
        string path = MigrationVariables.DisqusCommentsPath;

        if (File.Exists(path))
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(string.Empty, "http://disqus.com");
            nsmgr.AddNamespace("def", "http://disqus.com");
            nsmgr.AddNamespace("dsq", "http://disqus.com/disqus-internals");

            var threads = FindThreads(doc, nsmgr);
            var posts = FindPosts(doc, nsmgr);

            threads = MergeThreadsWithPosts(threads, posts);

            await SetMarkdownFileLocation(threads);

            //string json = JsonSerializer.Serialize(threads, new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //});

            //File.WriteAllText(@"C:\\Users\\baban\\Downloads\\disqus-comments.json", json);
        }
        else
        {
            Console.WriteLine("File does not exist");
        }
    }

    private static List<DisqusThread> FindThreads(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        if (doc.DocumentElement == null)
        {
            return [];
        }

        var xthreads = doc.DocumentElement.SelectNodes("def:thread", nsmgr);

        if (xthreads == null)
        {
            return [];
        }

        var threads = new List<DisqusThread>();
        var i = 0;

        foreach (XmlNode xthread in xthreads)
        {
            i++;

            long threadId = xthread.AttributeValue<long>(0);
            var title = xthread["title"]?.NodeValue() ?? string.Empty;
            var url = xthread["link"]?.NodeValue() ?? string.Empty;
            var isValid = CheckThreadUrl(url);
            var isDeleted = xthread["isDeleted"]?.NodeValue<bool>() ?? false;
            var isClosed = xthread["isClosed"]?.NodeValue<bool>() ?? false;
            var createdAt = xthread["createdAt"]?.NodeValue<DateTime>() ?? DateTime.MinValue;

            if (isDeleted || isClosed || !isValid)
            {
                continue;
            }

            threads.Add(new DisqusThread(threadId)
            {
                Title = HttpUtility.HtmlDecode(title),
                Url = url,
                CreatedAt = createdAt
            });
        }

        return threads;
    }

    private static bool CheckThreadUrl(string url)
    {
        if (!url.StartsWith("https://ssw.com.au/rules") && !url.StartsWith("https://www.ssw.com.au/rules"))
        {
            return false;
        }

        return true;
    }

    private static List<DisqusPost> FindPosts(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        if (doc.DocumentElement is null)
        {
            return [];
        }

        var xposts = doc.DocumentElement.SelectNodes("def:post", nsmgr);
        if (xposts == null)
        {
            return [];
        }

        var posts = new List<DisqusPost>();
        var i = 0;
        foreach (XmlNode xpost in xposts)
        {
            i++;

            var postId = xpost.AttributeValue<long>(0);
            var isDeleted = xpost["isDeleted"]?.NodeValue<bool>() ?? false;
            var isSpam = xpost["isSpam"]?.NodeValue<bool>() ?? false;
            var author = xpost["author"]?.ChildNodes[0]?.NodeValue() ?? string.Empty;
            var username = xpost["author"]?.ChildNodes[2]?.NodeValue() ?? string.Empty;
            var threadId = xpost["thread"]?.AttributeValue<long>(0) ?? 0;
            var parent = xpost["parent"]?.AttributeValue<long>(0) ?? 0;
            var message = xpost["message"]?.NodeValue() ?? string.Empty;
            var createdAt = xpost["createdAt"]?.NodeValue<DateTime>() ?? DateTime.MinValue;

            if (isDeleted || isSpam)
            {
                continue;
            }

            if (MigrationVariables.IgnoredUsers.Where(u => u.Equals(username)).Any())
            {
                continue;
            }

            var post = new DisqusPost(postId)
            {
                ThreadId = threadId,
                Parent = parent,
                Message = message,
                CreatedAt = createdAt,
                Author = author,
                Username = username
            };
            posts.Add(post);
        }

        return posts;
    }

    private static List<DisqusThread> MergeThreadsWithPosts(List<DisqusThread> threads, List<DisqusPost> posts)
    {
        foreach (var thread in threads)
        {
            var threadsPosts = posts
                .Where(x => x.ThreadId == thread.Id)
                .OrderBy(x => x.CreatedAt);

            thread.Posts.AddRange(threadsPosts);
        }

        threads = threads
            .Where(x => x.Posts.Count != 0)
            .OrderBy(x => x.CreatedAt)
            .ToList();
        return threads;
    }

    private static async Task SetMarkdownFileLocation(List<DisqusThread> threads)
    {
        var historyJsonUrl = "https://www.ssw.com.au/rules/history.json";
        var response = await _httpClient.GetAsync(historyJsonUrl);
        var rulesHistory = new List<RulesHistory>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            rulesHistory = JsonSerializer.Deserialize<List<RulesHistory>>(content) ?? [];
        }

        foreach (var thread in threads)
        {
            var uri = new Uri(thread.Url);
            var lastPart = uri.Segments.Last().Trim('/');

            thread.MarkdownFileLocation = rulesHistory
                .Where(rh => rh.File.Contains(lastPart, StringComparison.OrdinalIgnoreCase))
                .Select(rh => rh.File)
                .FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrEmpty(thread.MarkdownFileLocation))
            {
                Console.WriteLine($"The last part of url ({lastPart}) for thread ({thread.Id}) wasn't found in rules history");
            }
        }
    }
}