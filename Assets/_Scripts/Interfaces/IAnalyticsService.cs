using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Analytics;

namespace Interfaces
{
    public interface IAnalyticsService
    {
        public Task InitAnalytics();

        public void LogEvent<T>(EventParameters<T> eventParameters);
        public void LogEventParameterArray(string eventName, Dictionary<string, object> parameters);
        public bool IsFirebaseReady { get; set; }
    }

    /// <summary>
    /// The `EventParameters struct is a generic structure that holds three properties:
    /// `EventName`, `ParameterName`, and `ParameterValue`. Each of these properties is of the
    /// generic type `T`, allowing for flexibility in the types of data that can be stored within
    /// an instance of this struct. This struct is used to encapsulate event-related data for logging purposes.
    /// </summary>
    /// <typeparam name="T">The type of the event parameters.</typeparam>
    public struct EventParameters<T>
    {
        public string EventName;
        public string ParameterName;
        public T ParameterValue;
        public string AdjustToken; // Adjust token for the only adjust events
    }
}