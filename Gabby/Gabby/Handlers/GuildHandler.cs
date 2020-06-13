namespace Gabby.Handlers
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Gabby.Data;
    using Gabby.Models;
    using JetBrains.Annotations;

    public sealed class GuildHandler
    {
        public GuildHandler(
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] DiscordClient discord)
        {
            discord.GuildCreated += OnJoinedGuildAsync;
            discord.GuildDeleted += OnLeftGuildAsync;
            discord.GuildUpdated += OnGuildUpdatedAsync;
        }

        private static async Task OnLeftGuildAsync([NotNull] GuildDeleteEventArgs args)
        {
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(args.Guild.Id).ConfigureAwait(false);
            if (guild == null) return;

            await DynamoSystem.DeleteItemAsync(guild).ConfigureAwait(false);
        }

        private static async Task OnJoinedGuildAsync([NotNull] GuildCreateEventArgs guildCreateEventArgs)
        {
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(guildCreateEventArgs.Guild.Id).ConfigureAwait(false);
            if (guild != null) return;

            guild = new GuildInfo
            {
                GuildGuid = guildCreateEventArgs.Guild.Id.ToString(),
                GuildName = guildCreateEventArgs.Guild.Name
            };

            await DynamoSystem.PutItemAsync(guild).ConfigureAwait(false);
        }

        private static async Task OnGuildUpdatedAsync([NotNull] GuildUpdateEventArgs args)
        {
            var origGuildId = args.GuildBefore.Id;
            var guild = await DynamoSystem.GetItemAsync<GuildInfo>(origGuildId).ConfigureAwait(false);
            if (guild != null)
            {
                guild.GuildGuid = args.GuildAfter.Id.ToString();
                guild.GuildName = args.GuildAfter.Name;
                await DynamoSystem.UpdateItemAsync(guild).ConfigureAwait(false);
            }
        }
    }
}