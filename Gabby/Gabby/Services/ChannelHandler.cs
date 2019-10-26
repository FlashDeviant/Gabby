using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Gabby.Services
{
    public sealed class ChannelHandler
    {
        private readonly DiscordSocketClient _discord;

        public ChannelHandler(
            DiscordSocketClient discord)
        {
            _discord = discord;

            _discord.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        private async Task OnUserVoiceStateUpdated([NotNull] SocketUser user, SocketVoiceState oldVoiceState,
            SocketVoiceState newVoiceState)
        {
            if (user.Id == _discord.CurrentUser.Id) return;

            var guild = oldVoiceState.VoiceChannel != null
                ? oldVoiceState.VoiceChannel.Guild
                : newVoiceState.VoiceChannel.Guild;
            var oldVoiceChannel = oldVoiceState.VoiceChannel?.Name;
            var newVoiceChannel = newVoiceState.VoiceChannel?.Name;

            var oldRole = guild.Roles.SingleOrDefault(x => x.Name == oldVoiceChannel);
            var newRole = guild.Roles.SingleOrDefault(x => x.Name == newVoiceChannel);

            var guildUser = guild.GetUser(user.Id);
            if (oldRole != null) await guildUser.RemoveRoleAsync(oldRole);
            if (newRole != null) await guildUser.AddRoleAsync(newRole);
        }
    }
}
