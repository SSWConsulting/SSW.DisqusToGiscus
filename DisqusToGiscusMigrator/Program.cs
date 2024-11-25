using System.Text.Json;
using System.Web;
using System.Xml;
using DisqusToGiscusMigrator;
using DisqusToGiscusMigrator.Models;

public class Program
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task Main(string[] args)
    {
        string path = MigrationVariables.DisqusCommentsPath;
        Console.WriteLine(path);
        if (File.Exists(path))
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(String.Empty, "http://disqus.com");
            nsmgr.AddNamespace("def", "http://disqus.com");
            nsmgr.AddNamespace("dsq", "http://disqus.com/disqus-internals");

            var threads = await FindThreads(doc, nsmgr);
            var posts = FindPosts(doc, nsmgr);

            PrepareThreads(threads, posts);
        }
        else
        {
            Console.WriteLine("File does not exist");
        }
    }

    public static async Task<IEnumerable<DisqusThread>> FindThreads(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        if (doc.DocumentElement == null)
        {
            //Console.WriteLine("Document element is null");
            return Enumerable.Empty<DisqusThread>();
        }

        var xthreads = doc.DocumentElement.SelectNodes("def:thread", nsmgr);

        if (xthreads == null)
        {
            //Console.WriteLine("No threads found");
            return Enumerable.Empty<DisqusThread>();
        }

        var historyJsonUrl = "https://www.ssw.com.au/rules/history.json";
        var response = await _httpClient.GetAsync(historyJsonUrl);
        var rulesHistory = new List<RulesHistory>();

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            rulesHistory = JsonSerializer.Deserialize<List<RulesHistory>>(content) ?? new List<RulesHistory>();
        }

        var threads = new List<DisqusThread>();
        var i = 0;

        foreach (XmlNode xthread in xthreads)
        {
            i++;

            long threadId = xthread.AttributeValue<long>(0);
            var title = xthread["title"]?.NodeValue() ?? string.Empty;
            var url = xthread["link"]?.NodeValue() ?? string.Empty;
            var isValid = CheckThreadUrl(url, rulesHistory);
            var isDeleted = xthread["isDeleted"]?.NodeValue<bool>() ?? false;
            var isClosed = xthread["isClosed"]?.NodeValue<bool>() ?? false;
            var createdAt = xthread["createdAt"]?.NodeValue<DateTime>() ?? DateTime.MinValue;

            //Console.WriteLine($"{i:###} Found thread ({threadId}) '{title}'");

            if (isDeleted)
            {
                //Console.WriteLine($"{i:###} Thread ({threadId}) was deleted.");
                continue;
            }
            if (isClosed)
            {
                //Console.WriteLine($"{i:###} Thread ({threadId}) was closed.");
                continue;
            }
            if (!isValid)
            {
                //Console.WriteLine($"{i:###} the url Thread ({threadId}) is not valid: {url}");
                continue;
            }

            //Console.WriteLine($"{i:###} Thread ({threadId}) is valid");
            threads.Add(new DisqusThread(threadId)
            {
                Title = HttpUtility.HtmlDecode(title),
                Url = url,
                CreatedAt = createdAt
            });
        }

        return threads;
    }

    private static bool CheckThreadUrl(string url, List<RulesHistory> rulesHistory)
    {
        if (!url.StartsWith("https://ssw.com.au/rules") && !url.StartsWith("https://www.ssw.com.au/rules"))
        {
            return false;
        }

        //Uri uri = new(url);
        //string lastPartOfUrl = uri.Segments.Last().Trim('/').ToLower();
        //if (!rulesHistory.Where(r =>
        //    r.File.Contains(lastPartOfUrl, StringComparison.OrdinalIgnoreCase)).Any())
        //{
        //    Console.WriteLine($"last part of uri - {lastPartOfUrl}");
        //    Console.WriteLine($"url {url} not valid because it doesn't exists in history json");
        //    return false;
        //}

        return true;
    }

    private static IEnumerable<DisqusPost> FindPosts(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        if (doc.DocumentElement is null)
        {
            //Console.WriteLine("Document element is null");
            return Enumerable.Empty<DisqusPost>();
        }

        var xposts = doc.DocumentElement.SelectNodes("def:post", nsmgr);
        if (xposts == null)
        {
            //Console.WriteLine("No posts found");
            return Enumerable.Empty<DisqusPost>();
        }

        var posts = new List<DisqusPost>();
        var i = 0;
        foreach (XmlNode xpost in xposts)
        {
            i++;

            var postId = xpost.AttributeValue<long>(0);
            var isDeleted = xpost["isDeleted"]?.NodeValue<bool>() ?? false;
            var isSpam = xpost["isSpam"]?.NodeValue<bool>() ?? false;
            var author = xpost["author"]?.FirstChild?.NodeValue() ?? string.Empty;
            var threadId = xpost["thread"]?.AttributeValue<long>(0) ?? 0;
            var parent = xpost["parent"]?.AttributeValue<long>(0) ?? 0;
            var message = xpost["message"]?.NodeValue() ?? string.Empty;
            var createdAt = xpost["createdAt"]?.NodeValue<DateTime>() ?? DateTime.MinValue;

            //Console.WriteLine($"{i:###} found Post ({postId}) by {author}");

            if (isDeleted)
            {
                //Console.WriteLine($"{i:###} post ({postId}) was deleted");
                continue;
            }
            if (isSpam)
            {
                //Console.WriteLine($"{i:###} post ({postId}) was marked as spam");
                continue;
            }


            //Console.WriteLine($"{i:###} post ({postId}) is valid");

            var post = new DisqusPost(postId)
            {
                ThreadId = threadId,
                Parent = parent,
                Message = message,
                CreatedAt = createdAt,
                Author = author
            };
            posts.Add(post);
        }

        return posts;
    }

    private static void PrepareThreads(IEnumerable<DisqusThread> threads, IEnumerable<DisqusPost> posts)
    {
        foreach (var thread in threads)
        {
            var threadsPosts = posts
                .Where(x => x.ThreadId == thread.Id)
                .OrderBy(x => x.CreatedAt);

            thread.Posts.AddRange(threadsPosts);

            if (thread.Posts.Any())
            {
                Console.WriteLine($"Thread ({thread.Id}) '{thread.Title}' has {thread.Posts.Count} posts");
            }
        }

        threads = threads.Where(x => x.Posts.Any()).OrderBy(x => x.CreatedAt);
        Console.WriteLine(threads.Count());

        Console.WriteLine($"Total threads: {threads.Count()}");
    }
}
