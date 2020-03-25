namespace Gabby.Handlers
{
    using System.Threading.Tasks;
    using Discord.WebSocket;
    using Gabby.Modules;
    using Gabby.Services;
    using JetBrains.Annotations;

    public sealed class PairHandler
    {
        private readonly DiscordSocketClient _discord;

        public PairHandler(
            DiscordSocketClient discord)
        {
            this._discord = discord;

            this._discord.UserVoiceStateUpdated += this.OnUserVoiceStateUpdatedAsync;
        }

        private async Task OnUserVoiceStateUpdatedAsync([NotNull] SocketUser user, SocketVoiceState oldVoiceState,
            SocketVoiceState newVoiceState)
        {
            if (user.Id == this._discord.CurrentUser.Id || (oldVoiceState.VoiceChannel == newVoiceState.VoiceChannel)) return;

            await ChannelPairService.HandleChannelPair(user, oldVoiceState, newVoiceState, this._discord).ConfigureAwait(false);
        }
    }
}