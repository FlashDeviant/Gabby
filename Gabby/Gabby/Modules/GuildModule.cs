namespace Gabby.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DocumentModel;
    using Discord;
    using Discord.Commands;
    using Gabby.Data;
    using Gabby.Handlers;
    using Gabby.Models;
    using JetBrains.Annotations;

    [Name("Guild Commands")]
    [UsedImplicitly]
    public sealed class GuildModule : ModuleBase<SocketCommandContext>
    {
        [Command("serverinfo")]
        [Alias("si")]
        [Priority(2)]
        [RequireOwner]
        [UsedImplicitly]
        public async Task ServerInfoAsync(ulong? guid = null)
        {
            if (guid == null) guid = this.Context.Guild.Id;
            var info = await DynamoSystem.GetItemAsync<GuildInfo>(guid.ToString()).ConfigureAwait(false);

            Embed embed;
            if (info == null || string.IsNullOrEmpty(info.GuildGuid))
                embed = EmbedHandler.GenerateEmbedResponse(
                    "No Server was found with that GUID",
                    Color.Orange);
            else
                embed = EmbedHandler.GenerateEmbedResponse(
                    $"GuildGuid: {info.GuildGuid}\r\n" +
                    $"GuildName: {info.GuildName}");

            await this.ReplyAsync("", false, embed).ConfigureAwait(false);
        }

        [Command("serverinfo")]
        [Alias("si")]
        [Priority(1)]
        [RequireOwner]
        [UsedImplicitly]
        public async Task ServerInfoAsync([Remainder] string name)
        {
            var response = await DynamoSystem.QueryItemAsync<GuildInfo>("GuildName", QueryOperator.Equal, name)
                .ConfigureAwait(false);

            Embed embed;
            if (response.Count < 1)
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "No servers were found with that name",
                    Color.Orange);
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

            await this.ReplyAsync("", false, embed).ConfigureAwait(false);
        }

        [Command("addserverinfo")]
        [Alias("asi")]
        [RequireOwner]
        [UsedImplicitly]
        public async Task AddServerInfoAsync()
        {
            var response = await DynamoSystem.QueryItemAsync<GuildInfo>("GuildName", QueryOperator.Equal, this.Context.Guild.Name)
                .ConfigureAwait(false);

            if (response.Count < 1)
            {
                var existsResponse = EmbedHandler.GenerateEmbedResponse(
                    "This server already exists in the DB",
                    Color.Teal);
                await this.ReplyAsync("", false, existsResponse).ConfigureAwait(false);
                return;
            }

            var item = new GuildInfo
            {
                GuildGuid = this.Context.Guild.Id.ToString(),
                GuildName = this.Context.Guild.Name
            };
            await DynamoSystem.PutItemAsync(item).ConfigureAwait(false);

            var addedResponse = EmbedHandler.GenerateEmbedResponse(
                "This server has been added to the DB",
                Color.Green);

            await this.ReplyAsync("", false, addedResponse).ConfigureAwait(false);
        }
    }
}