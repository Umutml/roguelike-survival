using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Editor.SuperUnityBuildCustomActions.Utilities;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.SuperUnityBuildCustomActions.Tools
{
    [InitializeOnLoad]
    public class DiscordWebhookCommunication
    {
        // private static readonly string WebhookUrlTest = "https://ptb.discord.com/api/webhooks/1144312785451368489/8xdduwT3v3-YweKve5ibP7jfaCIz7I82T_T-I_lu0sLOlil2-Aks1JDgBqMvaw445YnF";

        // private static readonly string WebhookUrlSceneEdit = "https://ptb.discord.com/api/webhooks/1148186709213855755/tDtF88oprUN3CVzpG9jqo4vpk3bV0KdNRhiHkrw8Q7nAAN60ta7aZyOMwI_-R3ltNTT8";

        // private static readonly string WebhookUrlBuildAutomation = "https://ptb.discord.com/api/webhooks/1148187443053469807/Q6p3WbIGnbYsASvehfHKSITt03FAepzz4k2f-3A628wnbTjmk3IZOLGcdLjtvXICkuE4";

        private const string ProjectNameMarker = "NsRoguelike";


        public static async Task SendWebhook(string message)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new
                    {
                        content = $"{ProjectNameMarker}: {message}"
                    };

                    var json = JsonConvert.SerializeObject(payload);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(DiscordChannelWebhooks.Urls[DiscordChannelType.BuildAutomation], content);

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.Log($"{nameof(DiscordWebhookCommunication)}: Webhook message sent successfully!");
                    }
                    else
                    {
                        Debug.LogError($"{nameof(DiscordWebhookCommunication)}: Failed to send webhook message. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(DiscordWebhookCommunication)}: Exception occurred while sending webhook message. Exception: {ex.Message}");
            }
        }

        public static async Task SendWebhook(string message, string webhookUrl)
        {
            using (var client = new HttpClient())
            {
                var payload = new
                {
                    content = message
                };

                var json = JsonConvert.SerializeObject(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"{nameof(DiscordWebhookCommunication)}: Webhook message sent successfully!");
                }
                else
                {
                    Debug.LogError($"{nameof(DiscordWebhookCommunication)}: Failed to send webhook message. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
