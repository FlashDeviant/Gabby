namespace Gabby.Handlers
{
    using System.Threading.Tasks;
    using Discord.WebSocket;
    using Gabby.Data;
    using Gabby.Models;
    using JetBrains.Annotations;

    public sealed class GuildHandler
    {
        public GuildHandler(
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] DiscordSocketClient discord)
        {
            discord.JoinedGuild += OnJoinedGuildAsync;
            discord.LeftGuild += OnLeftGuildAsync;
            discord.GuildUpdated += OnGuildUpdatedAsync;
        }

        private static async Task OnLeftGuildAsync([NotNull] SocketGuild arg)
        {
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(arg.Id).ConfigureAwait(false);
            if (guild == null) return;

            await DynamoSystem.DeleteItemAsync(guild).ConfigureAwait(false);
        }

        private static async Task OnJoinedGuildAsync([NotNull] SocketGuild arg)
        {
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(arg.Id).ConfigureAwait(false);
            if (guild != null) return;

            guild = new GuildInfo
            {
                GuildGuid = arg.Id.ToString(),
                GuildName = arg.Name
            };

            await DynamoSystem.PutItemAsync(guild).ConfigureAwait(false);
        }

        private static async Task OnGuildUpdatedAsync([NotNull] SocketGuild arg1, [NotNull] SocketGuild arg2)
        {
            var origGuildId = arg1.Id;
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(origGuildId).ConfigureAwait(false);
            if (guild != null)
            {
                guild.GuildGuid = arg2.Id.ToString();
                guild.GuildName = arg2.Name;
                await DynamoSystem.UpdateItemAsync(guild).ConfigureAwait(false);
            }
        }
    }
}