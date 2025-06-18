namespace FirebaseTestApp.Services.Analytics;

public interface IAnalytics
{
    void SetUserId(string userId);
    void SetUserProperty(string name, string value);
    void TrackEvent(string eventName, params (string parameterName, object parameterValue)[] parameters);
    void TrackEvent(string eventName, IDictionary<string, object>? parameters);
}