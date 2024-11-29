using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Helpers;

namespace DisqusToGiscusMigrator;

public class Program
{
    private static readonly GitHubHelper _githubHelper = new();

    public static async Task Main()
    {
        var checkpointer = new Checkpointer(StaticSettings.CheckPointFilePath);
        var (migrationStatus, disqusBlogPosts) = checkpointer.TryLoad();

        if (migrationStatus == MigrationStatus.Unparsed)
        {
            disqusBlogPosts = XmlParser.Parse(StaticSettings.DisqusCommentsXmlPath);
            migrationStatus = MigrationStatus.ParsingCompleted;
            checkpointer.Checkpoint(migrationStatus, disqusBlogPosts);
        }

        if (migrationStatus == MigrationStatus.ParsingCompleted)
        {
            await RuleHelper.SetMarkdownFileLocation(disqusBlogPosts);
            await RuleHelper.SetGuid(disqusBlogPosts);
            migrationStatus = MigrationStatus.RuleInfoIsSet;
            checkpointer.Checkpoint(migrationStatus, disqusBlogPosts);
        }

        if (migrationStatus == MigrationStatus.RuleInfoIsSet)
        {
            await _githubHelper.AssociateDiscussions(disqusBlogPosts);
            migrationStatus = MigrationStatus.DiscussionsAssociated;
            checkpointer.Checkpoint(migrationStatus, disqusBlogPosts);
        }

        if (migrationStatus == MigrationStatus.DiscussionsAssociated)
        {
            // TODO
            Logger.Log("Add comments", LogLevel.Info);
            //migrationStatus = MigrationStatus.CommentsAssociated;
            //checkpointer.Checkpoint(migrationStatus, disqusBlogPosts);
        }

        Logger.Log("Migration is finished", LogLevel.Info);
    }
}