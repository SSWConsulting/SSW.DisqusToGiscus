using System.Xml;
using DisqusToGiscusMigrator.Models;
using DisqusToGiscusMigrator.Constants;
using System.Text.RegularExpressions;

namespace DisqusToGiscusMigrator.Helpers;

public static class XmlParser
{
    private static readonly Regex DisqusUserRegex = new(@"@([\w_\-0-9]+)\:disqus", RegexOptions.Compiled);

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

            var blogPosts = FindDisqusBlogPosts(doc, nsmgr);
            var comments = FindDisqusComments(doc, nsmgr);
            FixDisqusUserMentions(comments);

            var result = MergeThreadsWithPosts(blogPosts, comments);

            return result;
        }
        else
        {
            Logger.Log("Xml file does not exist", LogLevel.Warning);
            return [];
        }
    }

    private static IDictionary<long, DisqusBlogPost> FindDisqusBlogPosts(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.LogMethod(nameof(FindDisqusBlogPosts));
        var blogPosts = new Dictionary<long, DisqusBlogPost>();

        if (doc.DocumentElement == null)
        {
            return blogPosts;
        }

        var xthreads = doc.DocumentElement.SelectNodes("def:thread", nsmgr);
        if (xthreads == null)
        {
            return blogPosts;
        }

        var i = 0;
        foreach (XmlNode xthread in xthreads)
        {
            i++;

            var id = long.Parse(xthread.Attributes!.Item(0)!.Value!);
            var title = xthread["title"]?.InnerText ?? string.Empty;
            var url = xthread["link"]?.InnerText ?? string.Empty;
            var isDeleted = bool.Parse(xthread["isDeleted"]!.InnerText!);
            var isClosed = bool.Parse(xthread["isClosed"]!.InnerText!);
            var createdAt = DateTime.Parse(xthread["createdAt"]!.InnerText!);

            if (isDeleted || isClosed)
            {
                continue;
            }

            blogPosts.Add(id, new DisqusBlogPost(id, title, url, createdAt));
        }

        return blogPosts;
    }

    private static IDictionary<long, DisqusComment> FindDisqusComments(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        Logger.LogMethod(nameof(FindDisqusComments));
        var disqusComments = new Dictionary<long, DisqusComment>();

        if (doc.DocumentElement is null)
        {
            return disqusComments;
        }

        var xposts = doc.DocumentElement.SelectNodes("def:post", nsmgr);
        if (xposts == null)
        {
            return disqusComments;
        }

        var i = 0;
        foreach (XmlNode xpost in xposts)
        {
            i++;

            var id = long.Parse(xpost.Attributes!.Item(0)!.Value!);
            var blogPostId = long.Parse(xpost["thread"]!.Attributes!.Item(0)!.Value!);
            var parentId = long.TryParse(xpost["parent"]?.Attributes?.Item(0)?.Value, out var temp) ? temp : default;
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

            if (StaticSettings.IgnoredDisqusUsername.Where(u => u.Equals(authorUsername)).Any())
            {
                continue;
            }

            var post = new DisqusComment(
                id,
                blogPostId,
                parentId,
                message,
                createdAt,
                new Author(authorName, authorUsername)
            );

            disqusComments.Add(id, post);
        }

        return disqusComments;
    }

    private static List<DisqusBlogPost> MergeThreadsWithPosts(IDictionary<long, DisqusBlogPost> blogposts, IDictionary<long, DisqusComment> comments)
    {
        Logger.Log("Adding toplevel comments", LogLevel.Info);
        foreach (var comment in comments.Values.Where(x => x.ParentId == 0))
        {
            var blogPost = blogposts[comment.DisqusBlogPostId];
            blogPost.DisqusComments.Add(comment);
        }

        Logger.Log("Adding child comments", LogLevel.Info);
        foreach (var comment in comments.Values.Where(x => x.ParentId !=  0))
        {
            try
            {                
                var parentComment = comments[comment.ParentId];
                if (parentComment.ParentId != 0)
                {
                    Logger.Log($"Re-parenting comment {comment.Id}", LogLevel.Info);
                    while (parentComment.ParentId != 0)
                    {
                        parentComment = comments[parentComment.ParentId];
                    }
                }
                parentComment.ChildComments.Add(comment);
            }
            catch (Exception)
            {
                Logger.Log($"Error adding child comment {comment.Id} to parent {comment.ParentId}", LogLevel.Info);
                throw;
            }
        }

        var result = blogposts.Values
            .Where(blogPost => blogPost.DisqusComments.Count != 0 && IsValidUrl(blogPost.Url))
            .Select(blogPost =>
            {
                blogPost.DisqusComments = blogPost.DisqusComments
                .OrderBy(comment => comment.CreatedAt)
                .ToList();

                return blogPost;
            })
            .OrderBy(x => x.CreatedAt)
            .ToList();

        return result;
    }

    private static bool IsValidUrl(string url)
    {
        if (!url.StartsWith("https://ssw.com.au/rules") && !url.StartsWith("https://www.ssw.com.au/rules"))
        {
            return false;
        }

        return true;
    }

    private static void FixDisqusUserMentions(IDictionary<long, DisqusComment> comments)
    {
        var authors = comments.Values
            .Select(x => x.Author)
            .DistinctBy(x => x.Username)
            .ToDictionary(x => x.Username!, x => x);

        foreach (var comment in comments.Values)
        {
            if (DisqusUserRegex.Match(comment.Message) is { Success: true, Groups: { Count: 2} groups })
            {
                var username = groups[1].Value;
                var author = authors.TryGetValue(username, out var known)
                    ? known.AuthorAnchor
                    : username;

                comment.Message = DisqusUserRegex.Replace(comment.Message, author);
            }
        }
    }
}
