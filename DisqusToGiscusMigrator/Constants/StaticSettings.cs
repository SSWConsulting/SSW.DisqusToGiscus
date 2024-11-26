namespace DisqusToGiscusMigrator.Constants;

public static class StaticSettings
{
    public const string DisqusCommentsPath = @"C:\\Users\\baban\\Downloads\\disqus-comments.xml";

    public const string GitHubRepoRawPath = "https://raw.githubusercontent.com/SSWConsulting/SSW.Rules.Content/refs/heads/main/";

    public const string RulesHistoryJsonUrl = "https://www.ssw.com.au/rules/history.json";

    public const string GitHubOwner = "SSWConsulting";
    
    public const string GitHubRepo = "SSW.Rules.Content";

    public static readonly string[] IgnoredUsers =
    [
        ""
    ];

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