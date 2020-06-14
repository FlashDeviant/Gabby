namespace Gabby.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Gabby.Data;
    using Gabby.Handlers;
    using Gabby.Models;
    using JetBrains.Annotations;

    internal class ChannelPairService
    {
        public static async Task HandleChannelPair([NotNull] DiscordUser user, [NotNull] DiscordVoiceState oldVoiceState,
            [NotNull] DiscordVoiceState newVoiceState, DiscordClient discord)
        {
            var oldGuild = oldVoiceState?.Guild;
            var newGuild = newVoiceState?.Guild;

            var oldChannelGuid = oldVoiceState?.Channel == null ? string.Empty : oldVoiceState.Channel.Id.ToString();
            var newChannelGuid = newVoiceState?.Channel == null ? string.Empty : newVoiceState.Channel.Id.ToString();

            var oldPair = string.IsNullOrEmpty(oldChannelGuid) ? null : await DynamoSystem.GetItemAsync<ChannelPair>(oldChannelGuid);
            var newPair = string.IsNullOrEmpty(newChannelGuid) ? null : await DynamoSystem.GetItemAsync<ChannelPair>(newChannelGuid);

            if (oldPair == null && newPair == null) return;

            var oldGuildUser = oldGuild == null ? null : await oldGuild.GetMemberAsync(user.Id);
            var newGuildUser = newGuild == null ? null : await newGuild.GetMemberAsync(user.Id);

            if (oldPair != null && oldGuild != null)
            {
                var role = oldGuild?.Roles.SingleOrDefault(x => x.Value.Id.ToString() == oldPair.RoleGuid).Value;
                if (role != null) await oldGuildUser?.RevokeRoleAsync(role)!;
                var embed = EmbedHandler.GenerateEmbedResponse(
                    $"\u274C {user.Mention} has left {oldVoiceState?.Channel?.Name}",
                    DiscordColor.Red);
                await oldGuild?.Channels.Single(x => x.Value.Id.ToString() == oldPair.TextChannelGuid)
                    .Value.SendMessageAsync("", false, embed)!;
            }

            if (newPair != null && newGuild != null)
            {
                var role = newGuild?.Roles.SingleOrDefault(x => x.Value.Id.ToString() == newPair.RoleGuid).Value;
                if (role != null) await newGuildUser?.GrantRoleAsync(role)!;
                var embed = EmbedHandler.GenerateEmbedResponse(
                    $"\u2705 {user.Mention} has joined {newVoiceState?.Channel?.Name}",
                    DiscordColor.Green);
                await newGuild?.Channels.Single(x => x.Value.Id.ToString() == newPair.TextChannelGuid)
                    .Value.SendMessageAsync("", false, embed)!;
            }
        }
    }
}