using Microsoft.Maui.LifecycleEvents;

public static class MauiExceptions
{
    // We'll route all unhandled exceptions through this one event.
    public static event EventHandler<MauiUnhandledExceptionEventArgs> UnhandledException;

    public static MauiAppBuilder UseMauiExceptions(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) => {
                MauiExceptions.UnhandledException += HandleUnhandledException;
                CheckCrashReports();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                MauiExceptions.UnhandledException += HandleUnhandledException;
                CheckCrashReports();
            }));
#endif
        });
        
        return builder;
    }
    
    public class MauiUnhandledExceptionEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly Exception _exception;
        private readonly bool _isTerminating;

        public MauiUnhandledExceptionEventArgs(string message, Exception exception, bool isTerminating)
        {
            _message = message;
            _exception = exception;
            _isTerminating = isTerminating;
        }
        
        public string Message => _message;

        public Exception Exception => _exception;

        public bool IsTerminating => _isTerminating;
    }

    private static void HandleUnhandledException(object sender, MauiUnhandledExceptionEventArgs args)
    {
        PersistCrashReport(args);
        //Logger.LogCritical(exception: exception, "CRASH detected! (IsTerminating: {IsTerminating})", args.IsTerminating);
    }

    private static void PersistCrashReport(MauiUnhandledExceptionEventArgs args)
    {
        var errorFilePath = Path.Combine(
            FileSystem.Current.CacheDirectory,
            $"{DateTime.Now:yyyyMMdd_HHmmss}_CrashReport.txt");
        var errorMessage = string.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}\r\n{2}",
            DateTime.Now, args.Message, args.Exception.ToString()); //TODO stacktrace

        try
        {
            File.WriteAllText(errorFilePath, errorMessage);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to write crash report.");
        }
    }

    private static void CheckCrashReports()
    {
        Task.Run(async () =>
        {
            try
            {
                await CheckCrashReportsAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"Error checking crash reports");
            }
        }).ConfigureAwait(false);
    }

    private static async Task CheckCrashReportsAsync()
    {
        var errorDirectory = FileSystem.Current.CacheDirectory;
        if (string.IsNullOrEmpty(errorDirectory) || !Directory.Exists(errorDirectory))
        {
            return; // If the directory doesn't exist, return
        }

        // Find all files in the directory that match the naming pattern of crash reports
        var crashReportFiles = Directory.GetFiles(errorDirectory, "*_CrashReport.txt");

        // If no crash report files are found, return
        if (crashReportFiles.Length == 0)
        {
            return;
        }

        foreach (var errorFilePath in crashReportFiles)
        {
            try
            {
                // Read the contents of the crash report asynchronously
                var errorText = await File.ReadAllTextAsync(errorFilePath);

                // Log the crash report details
                Logger?.LogCritical(new FatalException(errorText), "CRASH detected from a previous session!");

                // Delete the crash report file asynchronously
                File.Delete(errorFilePath);

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                // Log any error that occurs while reading or deleting the file
                Logger?.LogError(ex, $"Error reporting crash from file: {errorFilePath}");
            }
        }
    }

    public class FatalException : Exception
    {
        public FatalException(string message) : base(message)
        {
        }
        
        public FatalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    private static ILogger? _logger;
    private static ILogger? Logger => _logger ??= Application.Current?.Handler?.MauiContext?.Services.GetService<ILoggerProvider>()?.CreateLogger("UnhandledException");

    static MauiExceptions()
    {
        // This is the normal event expected, and should still be used.
        // It will fire for exceptions from iOS and Mac Catalyst,
        // and for exceptions on background threads from WinUI 3.
        
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => 
        {
            if (args.ExceptionObject is Exception exception)
            {
                UnhandledException?.Invoke(sender,
                    new MauiUnhandledExceptionEventArgs("AppDomain.CurrentDomain.UnhandledException", exception,
                        args.IsTerminating));
            }
        }; 
        
        // Tasks that do not handle exceptions can be reported as UnobservedTaskException.
        // e.g.  These are exceptions in tasks that are not await-ed or Wait()-ed.
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            var isTerminating = true; // ?????
            UnhandledException?.Invoke(sender,
                new MauiUnhandledExceptionEventArgs("TaskScheduler.UnobservedTaskException", e.Exception,
                    isTerminating));
            e.SetObserved();
        };

#if IOS
        // iOS native exception handling. This may duplicate the fatal/crash error reporting, but
        // may handle unhandled exceptions that dotnet does not capture.
        ObjCRuntime.Runtime.MarshalManagedException +=
            (object sender, ObjCRuntime.MarshalManagedExceptionEventArgs args) =>
            {
                UnhandledException?.Invoke(sender,
                    new MauiUnhandledExceptionEventArgs(
                        $"ObjCRuntime.Runtime.MarshalManagedException [ExceptionMode: {args.ExceptionMode}]",
                        args.Exception,
                        true));
            };
        
        ObjCRuntime.Runtime.MarshalObjectiveCException +=
            (object sender, ObjCRuntime.MarshalObjectiveCExceptionEventArgs args) =>
            {
                var nativeStackTrace = string.Join("\n", args.Exception.CallStackSymbols);
                var ex = new Exception(
                    $"Objective-C exception: {args.Exception.Name} - {args.Exception.Reason}\nNative Stack:\n{nativeStackTrace}");
                UnhandledException?.Invoke(sender,
                    new MauiUnhandledExceptionEventArgs(
                        $"ObjCRuntime.Runtime.MarshalObjectiveCException [ExceptionMode: {args.ExceptionMode}]",
                        ex,
                        true));
            };

#elif ANDROID

        // For Android:
        // All exceptions will flow through Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser,
        // and NOT through AppDomain.CurrentDomain.UnhandledException

        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            UnhandledException?.Invoke(sender, new MauiUnhandledExceptionEventArgs("Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser", args.Exception, true));
        };
#endif
    }
}