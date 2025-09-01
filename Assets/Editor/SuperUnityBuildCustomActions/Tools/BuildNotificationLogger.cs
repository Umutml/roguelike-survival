using SuperUnityBuild.BuildTool;
using UnityEngine;

namespace Editor.SuperUnityBuildCustomActions.SharedActions
{
    // Call any method from here to log all notifications, warnings, and errors from the build process to the console
    // Call any method from here to log all notifications, warnings, and errors from the build process to the console
    // Call any method from here to log all notifications, warnings, and errors from the build process to the console
    public class BuildNotificationLogger
    {
        private static void LogNotifications()
        {
            var notifications = BuildNotificationList.instance.notifications;
            foreach (var notification in notifications)
            {
                Debug.Log($"Notification: {notification.title} - {notification.details}");
            }
        }

        private static void LogWarnings()
        {
            var warnings = BuildNotificationList.instance.warnings;
            foreach (var warning in warnings)
            {
                Debug.LogWarning($"Warning: {warning.title} - {warning.details}");
            }
        }

        private static void LogErrors()
        {
            var errors = BuildNotificationList.instance.errors;
            foreach (var error in errors)
            {
                Debug.LogError($"Error: {error.title} - {error.details}");
            }
        }

        public static void LogAll()
        {
            LogNotifications();
            LogWarnings();
            LogErrors();
        }
    }
}
