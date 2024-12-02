namespace DisqusToGiscusMigrator.Constants;

public static class StaticSettings
{
    public const string DisqusCommentsXmlPath = "{{PATH_TO_DISQUS_XML_FILE}}";

    public const string CheckPointFilePath = "{{PATH_TO_JSON_CHEKPOINT_FILE}}";

    public const string BotPAT = "{{BOT_PAT}}";

    public const string MainPAT = "{{MAIN_PAT}}";

    public const string BotGitHubUsername = "ssw-rules-comments-migrator";

    public const string ContentRepoRawPath = "https://raw.githubusercontent.com/SSWConsulting/SSW.Rules.Content/refs/heads/main/";

    public const string RulesHistoryJsonUrl = "https://www.ssw.com.au/rules/history.json";

    public const string RepoOwner = "SSWConsulting";
    
    public const string RepoName = "SSW.Rules.Staging.Discussions";

    public const string DiscussionCategory = "Test Migration";

    public const string DisqusForumLocation = "https://disqus.com/home/forum/sswrules/";

    public static readonly string[] IgnoredDisqusUsername =
    [
        "{{DISQUS_USERNAME}}"
    ];

    /// <summary>
    /// Use specified file location instead of getting it from history.json,
    /// as I faced issue where the location wasn't correct anymore due to url/folder name changes
    /// </summary>
    /// <key type="string">Disqus thread URL</key>    
    /// <value type="string">Markdown file location</value>
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
}