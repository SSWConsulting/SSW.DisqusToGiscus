using System.Xml;
using DisqusToGiscusMigrator.Models;
using DisqusToGiscusMigrator.Constants;
using System.Text.RegularExpressions;

namespace DisqusToGiscusMigrator.Helpers;

public static class XmlParser
{
    public static List<DisqusBlogPost> Parse(string path)
    {
        if (File.Exists(path))
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(string.Empty, "http://disqus.com");
            nsmgr.AddNamespace("def", "http://disqus.com");
            nsmgr.AddNamespace("dsq", "http://disqus.com/disqus-internals");

            var disqusBlogPosts = FindDisqusBlogPosts(doc, nsmgr);
            var disqusComments = FindDisqusComments(doc, nsmgr);
            disqusBlogPosts = MergeThreadsWithPosts(disqusBlogPosts, disqusComments);

            return disqusBlogPosts;
        }
        else
        {
            Console.WriteLine("File does not exist");
            return [];
        }
    }

    private static List<DisqusBlogPost> FindDisqusBlogPosts(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.LogMethod(nameof(FindDisqusBlogPosts));

        if (doc.DocumentElement == null)
        {
            return [];
        }

        var xthreads = doc.DocumentElement.SelectNodes("def:thread", nsmgr);
        if (xthreads == null)
        {
            return [];
        }

        var blogPosts = new List<DisqusBlogPost>();
        var i = 0;
        foreach (XmlNode xthread in xthreads)
        {
            i++;

            var id = long.Parse(xthread.Attributes!.Item(0)!.Value!);
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

            blogPosts.Add(new DisqusBlogPost(id, title, url, createdAt));
        }

        return blogPosts;
    }

    private static List<DisqusComment> FindDisqusComments(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.LogMethod(nameof(FindDisqusComments));

        if (doc.DocumentElement is null)
        {
            return [];
        }

        var xposts = doc.DocumentElement.SelectNodes("def:post", nsmgr);
        if (xposts == null)
        {
            return [];
        }

        var disqusComments = new List<DisqusComment>();
        var i = 0;
        foreach (XmlNode xpost in xposts)
        {
            i++;

            var id = long.Parse(xpost.Attributes!.Item(0)!.Value!);
            var blogPostId = long.Parse(xpost["thread"]!.Attributes!.Item(0)!.Value!);
            var parentId = long.Parse(xpost["parent"]?.Attributes?.Item(0)?.Value ?? "0");
            var isDeleted = bool.Parse(xpost["isDeleted"]!.InnerText);
            var isSpam = bool.Parse(xpost["isSpam"]!.InnerText);
            var authorNode = xpost["author"]!;
            var authorName = authorNode["name"]?.InnerText ?? "Anonymous";
            var authorUsername = authorNode["username"]?.InnerText ?? string.Empty;
            var message = (xpost["message"]?.FirstChild as XmlCDataSection)?.Value ?? string.Empty;
            var createdAt = DateTime.Parse(xpost["createdAt"]!.InnerText!);

            if (isDeleted || isSpam || string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            if (StaticSettings.IgnoredUsers.Where(u => u.Equals(authorUsername)).Any())
            {
                continue;
            }

            // resolve disqus mentions issue
            var pattern = @"@\w+:disqus";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            if (StaticSettings.DisqusMentions.TryGetValue(id, out var replaceWith) &&
                !string.IsNullOrEmpty(replaceWith))
            {
                message = regex.Replace(message, replaceWith);
            }

            // resolve multilevel reply comments as it is not supported in GitHub discussions
            if (StaticSettings.MultiLevelReplyComments.TryGetValue(id, out var newValue) &&
                newValue != default)
            {
                parentId = newValue;
            }

            var post = new DisqusComment(
                id,
                blogPostId,
                parentId,
                message,
                createdAt,
                authorName,
                authorUsername
            );

            disqusComments.Add(post);
        }

        return disqusComments;
    }

    private static List<DisqusBlogPost> MergeThreadsWithPosts(List<DisqusBlogPost> threads, List<DisqusComment> posts)
    {
        Logger.LogMethod(nameof(MergeThreadsWithPosts));

        foreach (var thread in threads)
        {
            var threadsPosts = posts
                .Where(x => x.DisqusBlogPostId == thread.Id)
                .OrderBy(x => x.CreatedAt);

            thread.DisqusComments.AddRange(threadsPosts);
        }

        threads = threads
            .Where(x => x.DisqusComments.Count != 0)
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
