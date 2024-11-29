using System.Security.Cryptography;
using System.Text;
using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Models;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace DisqusToGiscusMigrator.Helpers;

public class GitHubHelper
{
    private readonly Connection _botConnection;
    private readonly Connection _mainConnection;

    public GitHubHelper()
    {
        _botConnection = new Connection(new("Octokit.GraphQL.Net.SampleApp", "1.0"), StaticSettings.BotPAT);
        _mainConnection = new Connection(new("Octokit.GraphQL.Net.SampleApp", "1.0"), StaticSettings.MainPAT);
    }

    public async Task AssociateDiscussions(List<DisqusBlogPost> disqusBlogPosts)
    {
        Logger.LogMethod(nameof(AssociateDiscussions));
        var repoId = await GetRepositoryId();
        var categoryId = await GetCategoryId();

        foreach (var post in disqusBlogPosts.Where(x => x.AssociatedGitHubDiscussion is null))
        {
            try
            {
                var discussion = await FindDiscussionByGuid(post.Rule.Guid) ??
                                 await CreateDiscussion(post, repoId, categoryId);

                post.AssociatedGitHubDiscussion = discussion;
            }
            catch (Exception)
            {
                Logger.Log($"Error fetching/creating discussion for Disqus blog post ({post.Id})", LogLevel.Error);
                throw;
            }
        }
    }

    private async Task<GitHubDiscussion?> FindDiscussionByGuid(string guid)
    {
        Logger.LogMethod(nameof(FindDiscussionByGuid));
        var hash = guid.Sha1();
        GitHubDiscussion? discussion = null;
        try
        {
            var searchQuery = $"repo:{StaticSettings.RepoOwner}/{StaticSettings.RepoName} {hash} in:body";
            var query = new Query()
                .Search(searchQuery, SearchType.Discussion, first: 2)
                .Select(search => new
                {
                    search.DiscussionCount,
                    Discussion = search.Edges.Select(edge => new
                    {
                        Nodes = edge.Node.Select(node => node.Switch<GitHubDiscussion>(
                            when => when.Discussion(discussion => new GitHubDiscussion()
                            {
                                ID = discussion.Id.Value,
                                Title = discussion.Title,
                                Body = discussion.Body,
                                ID = discussion.Id,
                                Number = discussion.Number
                            })
                        )).SingleOrDefault()
                    }).ToList()
                }).Compile();

            var result = await _botConnection.Run(query);

            if (result.DiscussionCount == 0)
            {
                Logger.Log($"Discussion with guid ({guid}) not found", LogLevel.Warning);
                return discussion;
            }
            else if (result.DiscussionCount == 1)
            {
                discussion = result.Discussion.Select(x => x.Nodes).Single();
                Logger.Log($"Found discussion for rule\n{discussion.Body}", LogLevel.Info);
                return discussion;
            }
            else
            {
                Logger.Log($"Unexpectedly found {result.DiscussionCount} discussions for rule", LogLevel.Error);
                throw new Exception("Found multiple discussion for the rule");
            }
        }
        catch (Exception)
        {
            Logger.Log($"Error searching for discussion with rule guid ({guid})", LogLevel.Error);
            throw;
        }
    }

    private async Task<GitHubDiscussion> CreateDiscussion(DisqusBlogPost post, ID repoId, ID categoryId)
    {
        Logger.LogMethod(nameof(CreateDiscussion));
        try
        {
            var body = $"""
                {post.Url}

                <!-- sha1: {post.Rule.Guid.Sha1()} -->
                """;

            var mutation = new Mutation()
                .CreateDiscussion(new CreateDiscussionInput
                {
                    RepositoryId = repoId,
                    Title = post.Title,
                    Body = body,
                    CategoryId = categoryId
                })
                .Select(x => new GitHubDiscussion
                {
                    ID = x.Discussion.Id.Value,
                    Title = x.Discussion.Title,
                    Body = x.Discussion.Body,
                    ID = x.Discussion.Id,
                    Number = x.Discussion.Number
                });

            var discussion = await _botConnection.Run(mutation);
            if (discussion is null)
            {
                throw new Exception($"Failed to create discussion");
            }

            Logger.Log($"Created discussion for Disqus blog post ({post.Id})", LogLevel.Info);

            await Task.Delay(3_000);

            return discussion;
        }
        catch (Exception)
        {
            Logger.Log($"Error creating discussion for Disqus blog post ({post.Id})", LogLevel.Error);
            throw;
        }
    }

    /*
    private async Task DeleteDiscussion(ID id)
    {
        Logger.LogMethod(nameof(DeleteDiscussion));
        try
        {
            var mutation = new Mutation()
                .DeleteDiscussion(new DeleteDiscussionInput
                {
                    Id = id
                })
                .Select(x => x.Discussion.Id);

            await _mainConnection.Run(mutation);
            await Task.Delay(3_000);
            Logger.Log($"Deleted discussion with ID: {id.Value}", LogLevel.Info);
        }
        catch (Exception)
        {
            Logger.Log($"Error deleting discussion with ID: {id}", LogLevel.Error);
            throw;
        }
    }
    */

    private async Task<List<GitHubDiscussion>> GetAllDiscussions()
    {
        var categoryId = await GetCategoryId();
        try
        {
            var orderBy = new DiscussionOrder()
            {
                Direction = OrderDirection.Asc,
                Field = DiscussionOrderField.CreatedAt
            };

            var discussions = new List<GitHubDiscussion>();

            var query = new Query()
                .Repository(StaticSettings.RepoName, StaticSettings.RepoOwner)
                .Discussions(first: 100, categoryId: categoryId, orderBy: orderBy)
                .Edges.Select(e => new
                {
                    e.Node.Title,
                    e.Node.Body,
                    e.Node.Id,
                    e.Node.Number,
                    e.Node.Repository.Name
                })
                .Compile();

            var result = (await _botConnection.Run(query)).ToList();
            if (result.Count == 0)
            {
                return discussions;
            }

            foreach (var discussion in result)
            {
                discussions.Add(new GitHubDiscussion
                {
                    ID = discussion.Id.Value,
                    Title = discussion.Title,
                    Body = discussion.Body,
                    ID = discussion.Id,
                    Number = discussion.Number
                });
            }

            return discussions;
        }
        catch (Exception)
        {
            Logger.Log("Error fetching all discussions", LogLevel.Error);
            throw;
        }
    }

    private async Task<ID> GetRepositoryId()
    {
        Logger.LogMethod(nameof(GetRepositoryId));
        try
        {
            var query = new Query()
                .Repository(StaticSettings.RepoName, StaticSettings.RepoOwner)
                .Select(x => x.Id)
                .Compile();

            var id = await _botConnection.Run(query);
            return id;
        }
        catch (Exception)
        {
            Logger.Log("Error fetching repo ID", LogLevel.Error);
            throw;
        }
    }

    private async Task<ID> GetCategoryId()
    {
        Logger.LogMethod(nameof(GetCategoryId));
        try
        {
            var query = new Query()
                .Repository(StaticSettings.RepoName, StaticSettings.RepoOwner)
                .DiscussionCategories(first: 10)
                .Nodes
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .Compile();

            var result = await _botConnection.Run(query);

            var id = result.Single(x => x.Name.Equals(StaticSettings.DiscussionCategory)).Id;
            return id;
        }
        catch (Exception)
        {
            Logger.Log("Error fetching category ID", LogLevel.Error);
            throw;
        }
    }
}

public static class StringExtensions
{
    public static string Sha1(this string input)
    {
        var bytes = Encoding.ASCII.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
