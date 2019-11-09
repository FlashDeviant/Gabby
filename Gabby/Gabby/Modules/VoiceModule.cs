namespace Gabby.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;
    using Gabby.Data;
    using Gabby.Models;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal sealed class VoiceModule : ModuleBase<SocketCommandContext>
    {
        public static async Task HandleChannelPair([NotNull] SocketUser user, SocketVoiceState oldVoiceState,
            SocketVoiceState newVoiceState)
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

            if (oldPair != null)
            {
                var role = oldGuild?.Roles.SingleOrDefault(x => x.Id.ToString() == oldPair.RoleGuid);
                if (role != null) await oldGuildUser.RemoveRoleAsync(role);
            }

            if (newPair != null)
            {
                var role = newGuild?.Roles.SingleOrDefault(x => x.Id.ToString() == newPair.RoleGuid);
                if (role != null) await newGuildUser.AddRoleAsync(role);
            }
        }
    }
}