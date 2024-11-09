using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Text;
using ConVar;
using UnityEngine;


namespace Oxide.Plugins;

[Info("ChatMessageController", "Blitzmachine", "0.0.1")]
[Description("Sync your server/player chat messages in other plugins")]
public class ChatMessageController : RustPlugin
{
    #region Variables
    private PluginConfig config;
    #endregion

    #region Configuration

    #region Plugin Configuration Template
    private class PluginConfig
    {
        [JsonProperty(PropertyName = "Settings")]
        public ChatSettings Chat { get; set; }

        [JsonProperty(PropertyName = "Chat Title Settings")]
        public ChatPrefixSettings ChatPrefix { get; set; }

        [JsonProperty(PropertyName = "Chat Message Settings")]
        public ChatMessageSettings ChatMessage { get; set; }
    }

    private class ChatSettings
    {
        [JsonProperty(PropertyName = "Icon (Steam64 ID)")]
        public ulong SteamID { get; set; }

        [JsonProperty(PropertyName = "Chat Format")]
        public string Format { get; set; }
    }

    private class ChatPrefixSettings
    {
        [JsonProperty(PropertyName = "Prefix")]
        public string Prefix { get; set; }

        [JsonProperty(PropertyName = "Color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "Size")]
        public int Size { get; set; }
    }

    private class ChatMessageSettings
    {
        [JsonProperty(PropertyName = "Color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "Size")]
        public int Size { get; set; }
    }
    #endregion

    protected override void SaveConfig() => Config.WriteObject(config, true);
    protected override void LoadDefaultConfig()
    {
        config = new PluginConfig
        {
            Chat = new ChatSettings
            {
                SteamID = 76561199446355310,
                Format = "{prefix} » {message}"
            },
            ChatPrefix = new ChatPrefixSettings
            {
                Prefix = "Paradox Gaming",
                Color = "#d42f3f",
                Size = 15
            },
            ChatMessage = new ChatMessageSettings
            {
                Color = "#fff",
                Size = 13
            }
        };
    }

    protected override void LoadConfig()
    {
        base.LoadConfig();
        try
        {
            config = Config.ReadObject<PluginConfig>();
            PrintWarning("Configuration loaded!");
        }
        catch (Exception ex)
        {
            var builder = new StringBuilder("Failed to read Configuration (Invalid JSON Format):");
            builder.AppendLine(string.Empty);
            builder.AppendLine(ex.Message);

            PrintError(builder.ToString());
            PrintWarning("Using default values. Fix your configuration!");
            LoadDefaultConfig();
        }
    }
    #endregion

    #region Developer Hooks
    private void SendMessageToPlayer(BasePlayer player, string message)
    {
        if (player == null)
        {
            Puts("Could not send chat message to player. Player = null.");
            return;
        }

        Player.Message(player, FormatMessage(message), config.Chat.SteamID);
    }

    private void SendMessageToServer(string message)
    {
        if (BasePlayer.activePlayerList.Count > 0)
        {
            Server.Broadcast(FormatMessage(message), config.Chat.SteamID);
        }
    }
    #endregion

    #region Helper
    private string FormatMessage(string message)
    {
        return config.Chat.Format
            .Replace("{prefix}", GetStyledPrefix())
            .Replace("{message}", GetStyledMessage(message));
    }

    private string GetStyledPrefix()
    {
        return "<size={size}><color={color}>{prefix}</color></size>"
            .Replace("{size}", config.ChatPrefix.Size.ToString())
            .Replace("{color}", config.ChatPrefix.Color)
            .Replace("{prefix}", config.ChatPrefix.Prefix);
    }

    private string GetStyledMessage(string message)
    {
        return "<size={size}><color={color}>{message}</color></size>"
            .Replace("{size}", config.ChatMessage.Size.ToString())
            .Replace("{color}", config.ChatMessage.Color)
            .Replace("{message}", message);
    }
    #endregion
}
