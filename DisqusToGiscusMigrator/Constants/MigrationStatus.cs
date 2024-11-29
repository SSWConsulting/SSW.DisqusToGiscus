namespace DisqusToGiscusMigrator.Constants;

public enum MigrationStatus
{
    Unparsed,
    ParsingCompleted,
    RuleInfoIsSet,
    DiscussionsAssociated,
    CommentsAssociated
}