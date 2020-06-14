namespace Gabby.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DocumentModel;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Gabby.Data;
    using Gabby.Handlers;
    using Gabby.Models;
    using JetBrains.Annotations;

    [Group("guild")]
    [Aliases("g")]
    [UsedImplicitly]
    public sealed class GuildModule : BaseCommandModule
    {
        [Command("info")]
        [Priority(2)]
        [RequireOwner]
        [UsedImplicitly]
        public async Task ServerInfoAsync([NotNull] CommandContext ctx, ulong? guid = null)
        {
            guid ??= ctx.Guild.Id;
            var info = await DynamoSystem.GetItemAsync<GuildInfo>(guid.ToString()).ConfigureAwait(false);

            DiscordEmbed embed;
            if (info == null || string.IsNullOrEmpty(info.GuildGuid))
                embed = EmbedHandler.GenerateEmbedResponse(
                    "No Server was found with that GUID",
                    DiscordColor.Orange);
            else
                embed = EmbedHandler.GenerateEmbedResponse(
                    $"GuildGuid: {info.GuildGuid}\r\n" +
                    $"GuildName: {info.GuildName}");

            await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
        }

        [Command("info")]
        [Priority(1)]
        [RequireOwner]
        [UsedImplicitly]
        public async Task ServerInfoAsync(CommandContext ctx, [RemainingText] string name)
        {
            var response = await DynamoSystem.QueryItemAsync<GuildInfo>("GuildName", QueryOperator.Equal, name)
                .ConfigureAwait(false);

            DiscordEmbed embed;
            if (response.Count < 1)
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "No servers were found with that name",
                    DiscordColor.Orange);
            }
            else
            {
                var message = response.Aggregate("Found the following servers:\r\n",
                    (current, guildInfo) =>
                        current + $"GuildGuid: {guildInfo.GuildGuid}\r\n" +
                        $"GuildName: {guildInfo.GuildName}\r\n\r\n");

                embed = EmbedHandler.GenerateEmbedResponse(
                    message);
            }

            await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
        }

        [Command("addinfo")]
        [Aliases("ai")]
        [RequireOwner]
        [UsedImplicitly]
        public async Task AddServerInfoAsync([NotNull] CommandContext ctx)
        {
            var response = await DynamoSystem.QueryItemAsync<GuildInfo>("GuildName", QueryOperator.Equal, ctx.Guild.Name)
                .ConfigureAwait(false);

            if (response.Count < 1)
            {
                var existsResponse = EmbedHandler.GenerateEmbedResponse(
                    "This server already exists in the DB",
                    DiscordColor.Teal);
                await ctx.RespondAsync("", false, existsResponse).ConfigureAwait(false);
                return;
            }

            var item = new GuildInfo
            {
                GuildGuid = ctx.Guild.Id.ToString(),
                GuildName = ctx.Guild.Name
            };
            await DynamoSystem.PutItemAsync(item).ConfigureAwait(false);

            var addedResponse = EmbedHandler.GenerateEmbedResponse(
                "This server has been added to the DB",
                DiscordColor.Green);

            await ctx.RespondAsync("", false, addedResponse).ConfigureAwait(false);
        }
    }
}