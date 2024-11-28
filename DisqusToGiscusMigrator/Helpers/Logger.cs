namespace DisqusToGiscusMigrator.Helpers;

public class Logger
{
    public static void Log(string message, LogLevel level)
    {
        Console.ForegroundColor = level switch
        {
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
        Console.ResetColor();
    }

    public static void LogMethod(string method)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Running {method} ...");
    }
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}
