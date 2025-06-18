using Firebase.Crashlytics;

namespace FirebaseTestApp.Services.Logging.Firebase;

public class CrashlyticsException
{
    public static ExceptionModel Create(Exception exception, string message = null)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        var exceptionModel = new ExceptionModel(exception.GetType().ToString(), message ?? exception.Message) {
            StackTrace = StackTraceParser.Parse(exception)
                .Select(frame => new global::Firebase.Crashlytics.StackFrame(frame.Symbol, frame.FileName, frame.LineNumber))
                .ToArray()
        };
        return exceptionModel;
    }
}