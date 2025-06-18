using System.Diagnostics.CodeAnalysis;
using FirebaseTestApp.Services.Analytics.Firebase.Extensions;
using FirebaseAnalyticsIos = Firebase.Analytics.Analytics;

namespace FirebaseTestApp.Services.Analytics.Firebase;

[ExcludeFromCodeCoverage]
public class FirebaseAnalytics : IAnalytics
{
    public void SetUserId(string userId)
    {
        FirebaseAnalyticsIos.SetUserId(userId);
    }

    public void SetUserProperty(string name, string value)
    {
        FirebaseAnalyticsIos.SetUserProperty(name, value);
    }
    
    public void TrackEvent(string eventName, IDictionary<string, object>? parameters)
    {
        FirebaseAnalyticsIos.LogEvent(eventName, parameters?.ToNSDictionary());
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
