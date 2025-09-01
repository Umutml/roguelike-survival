using System.Collections.Generic;

namespace Editor.SuperUnityBuildCustomActions.Utilities
{
    public static class DiscordChannelWebhooks
    {
        public static Dictionary<DiscordChannelType, string> Urls = new() {
            {DiscordChannelType.FbIos,"https://ptb.discord.com/api/webhooks/1148241007801024553/v_xZN7RbCcjeNZbBo5tEeAV6GPTR7mvVEe8OjkD9S6nju_xPOdy4xO4gKayKkgvayIla"},
            {DiscordChannelType.FbAndroid,"https://ptb.discord.com/api/webhooks/1148241472357933086/65P_l-8RrKHm1c-dt82kbZduGz0zqSIW2htNdp_oE1L75FTh0kem3UUORQQcnUHTVPNk"},
            {DiscordChannelType.BuildAutomation,"https://ptb.discord.com/api/webhooks/1148187443053469807/Q6p3WbIGnbYsASvehfHKSITt03FAepzz4k2f-3A628wnbTjmk3IZOLGcdLjtvXICkuE4"},
            {DiscordChannelType.SceneEdit,"https://ptb.discord.com/api/webhooks/1148186709213855755/tDtF88oprUN3CVzpG9jqo4vpk3bV0KdNRhiHkrw8Q7nAAN60ta7aZyOMwI_-R3ltNTT8"},
            {DiscordChannelType.CiCdAndroid,"https://ptb.discord.com/api/webhooks/1144312066056929440/HUXyYlHEsavWlWVDgJZMTm0s9wxiuBOUxtamxQncqvNinn1veWGyxyUtAzA2o8DamFEj"},
            {DiscordChannelType.CiCdIos,"https://ptb.discord.com/api/webhooks/1144312066056929440/HUXyYlHEsavWlWVDgJZMTm0s9wxiuBOUxtamxQncqvNinn1veWGyxyUtAzA2o8DamFEj"}
         };
    }
}
