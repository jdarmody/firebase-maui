using System.Diagnostics.CodeAnalysis;
using FirebaseTestApp.Services.Analytics.Firebase.Extensions;
using GoogleFirebaseAnalytics = Firebase.Analytics.FirebaseAnalytics;

namespace FirebaseTestApp.Services.Analytics.Firebase;

[ExcludeFromCodeCoverage]
public class FirebaseAnalytics : IAnalytics
{
    private readonly GoogleFirebaseAnalytics _firebaseAnalytics = GoogleFirebaseAnalytics.GetInstance(Platform.AppContext);

    public void SetUserId(string userId)
    {
        _firebaseAnalytics.SetUserId(userId);
    }

    public void SetUserProperty(string name, string value)
    {
        _firebaseAnalytics.SetUserProperty(name, value);
    }
    
    public void TrackEvent(string eventName, IDictionary<string, object>? parameters)
    {
        _firebaseAnalytics.LogEvent(eventName, parameters?.ToBundle());
        System.Diagnostics.Debug.WriteLine(
            parameters != null
                ? $"Firebase Analytics: {eventName}, parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}"
                : $"Firebase Analytics: {eventName}");
    }

    public void TrackEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
    {
        TrackEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
    }
}
