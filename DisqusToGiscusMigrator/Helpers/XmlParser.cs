using System.Xml;
using DisqusToGiscusMigrator.Models;
using DisqusToGiscusMigrator.Constants;
using System.Text.RegularExpressions;

namespace DisqusToGiscusMigrator.Helpers;

public static class XmlParser
{
    public static List<DisqusThread> Parse(string path)
    {
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

            return threads;
        }
        else
        {
            Console.WriteLine("File does not exist");
            return [];
        }
    }

    private static List<DisqusThread> FindThreads(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.Log(nameof(FindThreads));

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

            var threadId = long.Parse(xthread.Attributes!.Item(0)!.Value!);
            var title = xthread["title"]?.InnerText ?? string.Empty;
            var url = xthread["link"]?.InnerText ?? string.Empty;
            var isValid = CheckThreadUrl(url);
            var isDeleted = bool.Parse(xthread["isDeleted"]!.InnerText!);
            var isClosed = bool.Parse(xthread["isClosed"]!.InnerText!);
            var createdAt = DateTime.Parse(xthread["createdAt"]!.InnerText!);

            if (isDeleted || isClosed || !isValid)
            {
                continue;
            }

            threads.Add(new DisqusThread(threadId)
            {
                Title = title,
                Url = url,
                CreatedAt = createdAt
            });
        }

        return threads;
    }

    private static List<DisqusPost> FindPosts(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.Log(nameof(FindPosts));

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

            var postId = long.Parse(xpost.Attributes!.Item(0)!.Value!);
            var threadId = long.Parse(xpost["thread"]!.Attributes!.Item(0)!.Value!);
            var parentId = long.Parse(xpost["parent"]?.Attributes?.Item(0)?.Value ?? "0");
            var isDeleted = bool.Parse(xpost["isDeleted"]!.InnerText);
            var isSpam = bool.Parse(xpost["isSpam"]!.InnerText);
            var authorNode = xpost["author"]!;
            var author = authorNode["name"]?.InnerText ?? "Anonymous";
            var username = authorNode["username"]?.InnerText ?? string.Empty;
            var message = (xpost["message"]?.FirstChild as XmlCDataSection)?.Value ?? string.Empty;
            var createdAt = DateTime.Parse(xpost["createdAt"]!.InnerText!);

            if (isDeleted || isSpam || string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            if (StaticSettings.IgnoredUsers.Where(u => u.Equals(username)).Any())
            {
                continue;
            }

            // resolve disqus mentions issue
            var pattern = @"@\w+:disqus";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            if (StaticSettings.DisqusMentions.TryGetValue(postId, out var replaceWith) &&
                !string.IsNullOrEmpty(replaceWith))
            {
                message = regex.Replace(message, replaceWith);
            }

            // resolve multilevel reply comments as it is not supported in GitHub discussions
            if (StaticSettings.MultiLevelReplyComments.TryGetValue(postId, out var newValue) &&
                newValue != default)
            {
                parentId = newValue;
            }

            var post = new DisqusPost(postId)
            {
                ThreadId = threadId,
                Parent = parentId,
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
        Logger.Log(nameof(MergeThreadsWithPosts));

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

    private static bool CheckThreadUrl(string url)
    {
        if (!url.StartsWith("https://ssw.com.au/rules") && !url.StartsWith("https://www.ssw.com.au/rules"))
        {
            return false;
        }

        return true;
    }
}
