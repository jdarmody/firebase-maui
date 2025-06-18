using FirebaseCrashlytics = Firebase.Crashlytics;

namespace FirebaseTestApp.Services.Logging.Firebase;

public class FirebaseLogger(string categoryName) : ILogger
{
    internal string CategoryName { get; } = categoryName;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            System.Diagnostics.Debug.WriteLine($"Firebase Logger: Ignoring log with {logLevel}");
            return;
        }
        
        var message = formatter.Invoke(state, exception);
        
        if (exception is not null)
        {
            FirebaseCrashlytics.Crashlytics.SharedInstance.RecordExceptionModel(CrashlyticsException.Create(exception, message));
            System.Diagnostics.Debug.WriteLine($"Firebase Logger: Reported exception: {exception}");
        }
        else
        {
            FirebaseCrashlytics.Crashlytics.SharedInstance.Log(message);
            System.Diagnostics.Debug.WriteLine($"Firebase Logger: Sent log message: {message}");
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == LogLevel.Critical || logLevel == LogLevel.Error;
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
}