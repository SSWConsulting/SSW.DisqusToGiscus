namespace DisqusToGiscusMigrator.Constants;

public static class StaticSettings
{
    public const string DisqusCommentsPath = "{{PATH_TO_DISQUS_XML_FILE}}";
    
    public const string BotPAT = "{{PLACEHOLDER}}";

    public const string MainPAT = "{{PLACEHOLDER}}";

    public const string BotGitHubUsername = "ssw-rules-comments-migrator";

    public const string ContentRepoRawPath = "https://raw.githubusercontent.com/SSWConsulting/SSW.Rules.Content/refs/heads/main/";

    public const string RulesHistoryJsonUrl = "https://www.ssw.com.au/rules/history.json";

    public const string RepoOwner = "SSWConsulting";
    
    public const string RepoName = "SSW.Rules.Staging.Discussions";

    public const string DiscussionCategory = "Test Migration";

    public static readonly string[] IgnoredDisqusUsername =
    [
        "{{PLACEHOLDER}}"
    ];

    /// <summary>
    /// <key>Disqus thread URL <see cref="string"/></key>    
    /// <value>Markdown file location <see cref="string"/></value>
    /// <description>
    /// Use specified file location instead of getting it from history.json,
    /// as I faced issue where the location wasn't correct anymore due to url/folder name changes
    /// </description>
    /// </summary>
    public static Dictionary<string, string> ForcedFileLocations { get; } = new()
    {
        { "https://ssw.com.au/rules/do-you-understand-the-value-of-consistency", "rules/the-value-of-consistency/rule.md" },
        { "https://www.ssw.com.au/rules/extending-AD", "rules/search-employee-skills/rule.md" },
        { "https://www.ssw.com.au/rules/do-you-zz-old-files-rather-than-deleting-them", "rules/zz-files/rule.md" },
        { "https://www.ssw.com.au/rules/do-you-label-broken-equipment", "rules/label-broken-equipment/rule.md" },
        { "https://www.ssw.com.au/rules/meetings-are-you-prepared-for-the-initial-meeting", "rules/prepare-for-initial-meetings/rule.md" },
        { "https://www.ssw.com.au/rules/is-everyone-in-your-team-a-standards-watchdog", "rules/standards-watchdog/rule.md" },
        { "https://www.ssw.com.au/rules/do-you-keep-your-npm-packages-up-to-date", "rules/packages-up-to-date/rule.md" },
        { "https://www.ssw.com.au/rules/formal-or-informal-mentoring-program", "rules/mentoring-programs/rule.md" },
        { "https://www.ssw.com.au/rules/do-you-fix-problems-quickly", "rules/fix-problems-quickly/rule.md" },
        { "https://www.ssw.com.au/rules/manage-azure-costs", "rules/reduce-azure-costs/rule.md" },
        { "https://www.ssw.com.au/rules/how-to-indicate-replaceable-text", "rules/placeholder-for-replaceable-text/rule.md" },
        { "https://ssw.com.au/rules/know-the-basic-video-terminologies", "rules/video-editing-terms/rule.md" },
        { "https://ssw.com.au/rules/good-candidate-for-automation", "rules/good-candidate-for-test-automation/rule.md" },
        { "https://www.ssw.com.au/rules/do-you-use-dev-tunnels-to-test-local-builds/", "rules/port-forwarding/rule.md" },
        { "https://ssw.com.au/rules/monetize-gpt-models/", "rules/create-gpts/rule.md" },
        { "https://www.ssw.com.au/rules/dns-what-and-how-it-works/", "rules/what-is-dns/rule.md" },
        { "https://www.ssw.com.au/rules/manage-urges/", "rules/separate-urge-from-behavior/rule.md" }
    };

    /// <summary>
    /// <key>Id of Disqus comment which has mentions(@username:discus) in the message property <see cref="long"/></key>
    /// <value>Replace the mentions with the specified value <see cref="string"/></value>
    /// <description>
    /// To avoid broken links in GitHub discussions, because when "@username:disqus" is added
    /// to GitHub discussion, it will be the link to the GitHub user.
    /// </description> 
    /// </summary>
    public static Dictionary<long, string> DisqusMentions { get; } = new()
    {
        { 6105939616, "{{PLACEHOLDER}}" },
        { 6201620423, "{{PLACEHOLDER}}" }
    };

    /// <summary>
    /// <key>Id of Disqus miltilevel reply comment <see cref="long"/></key>
    /// <value>New value for Parent Id property of multilevel comment <see cref="long"/></value>
    /// <description>Multilevel reply comments is not supported in GitHub discussions</description>
    /// </summary>
    public static Dictionary<long, long> MultiLevelReplyComments { get; } = new()
    {
        { 5492294233, 5491639342 },
        { 6292194250, 6288502889 }
    };
}