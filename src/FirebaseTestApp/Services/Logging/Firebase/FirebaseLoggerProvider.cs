using System.Collections.Concurrent;

namespace FirebaseTestApp.Services.Logging.Firebase;

public class FirebaseLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FirebaseLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FirebaseLogger(categoryName));
    
    public void Dispose()
    {
        _loggers.Clear();
    }
}