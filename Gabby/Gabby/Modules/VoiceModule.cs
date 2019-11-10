namespace Gabby.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Gabby.Data;
    using Gabby.Models;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal sealed class VoiceModule : ModuleBase<SocketCommandContext>
    {
        public static async Task HandleChannelPair([NotNull] SocketUser user, SocketVoiceState oldVoiceState,
            SocketVoiceState newVoiceState, DiscordSocketClient discord)
        {
            var oldGuild = oldVoiceState.VoiceChannel?.Guild;
            var newGuild = newVoiceState.VoiceChannel?.Guild;

            var oldChannelGuid = oldVoiceState.VoiceChannel == null ? string.Empty : oldVoiceState.VoiceChannel.Id.ToString();
            var newChannelGuid = newVoiceState.VoiceChannel == null ? string.Empty : newVoiceState.VoiceChannel.Id.ToString();

            var oldPair = string.IsNullOrEmpty(oldChannelGuid) ? null : await DynamoSystem.GetItemAsync<ChannelPair>(oldChannelGuid);
            var newPair = string.IsNullOrEmpty(newChannelGuid) ? null : await DynamoSystem.GetItemAsync<ChannelPair>(newChannelGuid);

            if (oldPair == null && newPair == null) return;

            var oldGuildUser = oldGuild?.GetUser(user.Id);
            var newGuildUser = newGuild?.GetUser(user.Id);

            if (oldPair != null && oldGuild != null)
            {
                var role = oldGuild?.Roles.SingleOrDefault(x => x.Id.ToString() == oldPair.RoleGuid);
                if (role != null) await oldGuildUser.RemoveRoleAsync(role);
                var embed = MessageModule.GenerateEmbedResponse(
                    $"\u274C {user.Username} has left {oldVoiceState.VoiceChannel?.Name}",
                    Color.Red);
                await oldGuild.TextChannels.Single(x => x.Id.ToString() == oldPair.TextChannelGuid)
                    .SendMessageAsync("", false, embed);
            }

            if (newPair != null && newGuild != null)
            {
                var role = newGuild?.Roles.SingleOrDefault(x => x.Id.ToString() == newPair.RoleGuid);
                if (role != null) await newGuildUser.AddRoleAsync(role);
                var embed = MessageModule.GenerateEmbedResponse(
                    $"\u2705 {user.Username} has joined {newVoiceState.VoiceChannel?.Name}",
                    Color.Green);
                await newGuild.TextChannels.Single(x => x.Id.ToString() == newPair.TextChannelGuid)
                    .SendMessageAsync("", false, embed);
            }
        }
    }
}