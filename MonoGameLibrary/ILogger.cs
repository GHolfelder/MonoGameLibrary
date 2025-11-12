namespace MonoGameLibrary;

/// <summary>
/// Interface for logging messages
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message
    /// </summary>
    void LogInfo(string message);
    
    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message);
    
    /// <summary>
    /// Logs an error message
    /// </summary>
    void LogError(string message);
}

/// <summary>
/// Default logger that does nothing (silent behavior)
/// </summary>
public class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new NullLogger();
    
    private NullLogger() { }
    
    public void LogInfo(string message) { }
    public void LogWarning(string message) { }
    public void LogError(string message) { }
}

/// <summary>
/// Debug logger that writes to Console (only in debug builds)
/// </summary>
public class ConsoleLogger : ILogger
{
    public static readonly ConsoleLogger Instance = new ConsoleLogger();
    
    private ConsoleLogger() { }
    
    public void LogInfo(string message)
    {
#if DEBUG
        System.Console.WriteLine($"[INFO] {message}");
#endif
    }
    
    public void LogWarning(string message)
    {
#if DEBUG
        System.Console.WriteLine($"[WARN] {message}");
#endif
    }
    
    public void LogError(string message)
    {
        System.Console.WriteLine($"[ERROR] {message}");
    }
}
