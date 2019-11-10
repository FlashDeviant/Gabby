namespace Gabby.Modules
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Gabby.Data;
    using Gabby.Models;
    using JetBrains.Annotations;

    [Name("Channel Pairs")]
    [UsedImplicitly]
    public sealed class ChannelModule : ModuleBase<SocketCommandContext>
    {
        /// <exception cref="T:System.InvalidOperationException">
        ///     No element satisfies the condition in <paramref name="predicate">predicate</paramref>.
        ///     -or-
        ///     The source sequence is empty.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="source">source</paramref> or
        ///     <paramref name="predicate">predicate</paramref> is null.
        /// </exception>
        [Command("createchannelpair")]
        [Alias("ccp")]
        [Summary("Create a text/voice channel pair with a role to control whether the name channel is visible")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task CreateChannelPairAsync([Remainder] [NotNull] string name)
        {
            Embed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters.\r\n" +
                    "\r\n" +
                    "Please make sure your name only uses alphanumeric characters or spaces",
                    Color.Red);

                await this.ReplyAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var textChannelName = name.Trim().ToLower().Replace(" ", "-");

            if (this.Context.Guild.Channels.Any(x => x.Name == name))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a voice or text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await this.ReplyAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            if (this.Context.Guild.Channels.Any(x => x.Name == textChannelName))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await this.ReplyAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            if (this.Context.Guild.Roles.Any(x => x.Name == name))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a role exists with that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await this.ReplyAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var textChannel = await this.Context.Guild.CreateTextChannelAsync(textChannelName).ConfigureAwait(false);
            var voiceChannel = await this.Context.Guild.CreateVoiceChannelAsync(name.Trim()).ConfigureAwait(false);

            var newRole = await this.Context.Guild.CreateRoleAsync(name.Trim()).ConfigureAwait(false);
            var everyoneRole =
                this.Context.Guild.GetRole(this.Context.Guild.Roles.First(x => x.Name == "@everyone").Id);

            await textChannel.AddPermissionOverwriteAsync(newRole,
                    new OverwritePermissions(viewChannel: PermValue.Allow))
                .ConfigureAwait(false);
            await textChannel.AddPermissionOverwriteAsync(everyoneRole,
                    new OverwritePermissions(viewChannel: PermValue.Deny))
                .ConfigureAwait(false);
            await textChannel.AddPermissionOverwriteAsync(this.Context.User,
                new OverwritePermissions(viewChannel: PermValue.Allow));

            var pair = new ChannelPair
            {
                VoiceChannelGuid = voiceChannel.Id.ToString(),
                RoleGuid = newRole.Id.ToString(),
                TextChannelGuid = textChannel.Id.ToString(),
                GuildGuid = this.Context.Guild.Id.ToString(),
                CreationDate =
                    DateTime.UtcNow.ToString(CultureInfo.CurrentCulture),
                Creator = this.Context.User.Id.ToString()
            };
            await DynamoSystem.PutItemAsync(pair).ConfigureAwait(false);

            embed = MessageModule.GenerateEmbedResponse(
                "I made your Channel Pair, Yay! Order them wherever you like \uD83D\uDE42\r\n" +
                "Make sure to not rename either of the channels or the role they are associated with",
                Color.Green);

            await this.ReplyAsync("", false, embed).ConfigureAwait(false);
        }

        /// <exception cref="T:System.ArgumentNullException"><paramref name="oldValue">oldValue</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="oldValue">oldValue</paramref> is the empty string (&amp;
        ///     quot;&amp;quot;).
        /// </exception>
        [Command("removechannelpair")]
        [Alias("rcp")]
        [Summary("Remove a channel pair from the server (or any of it's remains if it's been partially deleted)''")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task RemoveChannelPairAsync([Remainder] [NotNull] string name)
        {
            Embed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters \uD83D\uDE2E, that won't exist silly :P.\r\n" +
                    "\r\n" +
                    "It should use only alphanumeric characters and spaces.",
                    Color.Red);

                await this.ReplyAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var voiceChannelResults = this.Context.Guild.VoiceChannels.Where(x => x.Name == name).ToList();
            if (voiceChannelResults.Count > 1)
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oh no! I found multiple paired voice channels with that name\r\n" +
                    "Please try renaming the one you you meant to something unique and try again");
                await this.ReplyAsync("", false, embed);
                return;
            }

            var pairToRemove = await DynamoSystem.GetItemAsync<ChannelPair>(voiceChannelResults[0].Id);

            await this.Context.Guild.Channels.Single(x => x.Id.ToString() == pairToRemove.TextChannelGuid)
                .DeleteAsync();
            await this.Context.Guild.VoiceChannels.Single(x => x.Id.ToString() == pairToRemove.VoiceChannelGuid)
                .DeleteAsync();
            await this.Context.Guild.Roles.Single(x => x.Id.ToString() == pairToRemove.RoleGuid)
                .DeleteAsync();

            embed = MessageModule.GenerateEmbedResponse(
                "I packed up the channel pair you gave me and sent it on it's way, so long! \uD83D\uDE22",
                Color.Green);

            await this.ReplyAsync("", false, embed).ConfigureAwait(false);
        }
    }
}