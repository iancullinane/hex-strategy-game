using Godot;

public enum LogLevel
{
    None = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Verbose = 5
}

public static class GameLogger
{
    private static LogLevel currentLogLevel = LogLevel.Info;
    private static bool debugMode = false;
    private static bool verboseLogging = false;

    public static void Initialize(GameConfig config)
    {
        debugMode = config.DebugMode;
        verboseLogging = config.VerboseLogging;

        if (verboseLogging)
        {
            currentLogLevel = LogLevel.Verbose;
        }
        else if (debugMode)
        {
            currentLogLevel = LogLevel.Debug;
        }
        else
        {
            currentLogLevel = LogLevel.Info;
        }

        Info("GameLogger initialized", $"LogLevel: {currentLogLevel}, Debug: {debugMode}, Verbose: {verboseLogging}");
    }

    public static void Error(string context, string message = "")
    {
        if (currentLogLevel >= LogLevel.Error)
        {
            LogMessage("ERROR", context, message);
        }
    }

    public static void Warning(string context, string message = "")
    {
        if (currentLogLevel >= LogLevel.Warning)
        {
            LogMessage("WARN", context, message);
        }
    }

    public static void Info(string context, string message = "")
    {
        if (currentLogLevel >= LogLevel.Info)
        {
            LogMessage("INFO", context, message);
        }
    }

    public static void Debug(string context, string message = "")
    {
        if (currentLogLevel >= LogLevel.Debug)
        {
            LogMessage("DEBUG", context, message);
        }
    }

    public static void Verbose(string context, string message = "")
    {
        if (currentLogLevel >= LogLevel.Verbose)
        {
            LogMessage("VERBOSE", context, message);
        }
    }

    private static void LogMessage(string level, string context, string message)
    {
        string logMessage = string.IsNullOrEmpty(message)
            ? $"[{level}] {context}"
            : $"[{level}] {context}: {message}";

        GD.Print(logMessage);
    }
}