using FirebaseTestApp.Services.Analytics;
using FirebaseTestApp.Services.Analytics.Firebase;
using FirebaseTestApp.Services.Logging.Firebase;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace FirebaseTestApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiExceptions()
			.SetupLogging()
			.UseFirebase()
			.RegisterServices()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


		return builder.Build();
	}

	private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<IAnalytics, FirebaseAnalytics>();
		return builder;	
	}

	private static MauiAppBuilder SetupLogging(this MauiAppBuilder builder)
	{
		builder.Logging.AddConsole();
		builder.Logging.AddProvider(new FirebaseLoggerProvider());
#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder;	
	}

	private static MauiAppBuilder UseFirebase(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) => {
	            Firebase.Core.App.Configure ();
	            Firebase.Crashlytics.Crashlytics.SharedInstance.SetCrashlyticsCollectionEnabled(true);
	            Firebase.Crashlytics.Crashlytics.SharedInstance.SendUnsentReports();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) => {
	            Firebase.FirebaseApp.InitializeApp(activity);
	            Firebase.Crashlytics.FirebaseCrashlytics.Instance.SetCrashlyticsCollectionEnabled(Java.Lang.Boolean.True);
	            Firebase.Crashlytics.FirebaseCrashlytics.Instance.SendUnsentReports();
            }));
#endif
        });
        
        return builder;
    }
}
