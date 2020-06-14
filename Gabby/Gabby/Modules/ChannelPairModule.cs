namespace Gabby.Modules
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Gabby.Data;
    using Gabby.Handlers;
    using Gabby.Models;
    using JetBrains.Annotations;

    [Group("channel-pair")]
    [Aliases("pair")]
    [UsedImplicitly]
    public sealed class ChannelPairModule : BaseCommandModule
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
        [Command("create")]
        [Aliases("c")]
        [Description("Create a text/voice channel pair with a role to control whether the name channel is visible")]
        [RequirePermissions(Permissions.ManageChannels)]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [UsedImplicitly]
        public async Task CreateChannelPairAsync(CommandContext ctx, [RemainingText] [NotNull] string name)
        {
            DiscordEmbed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters.\r\n" +
                    "\r\n" +
                    "Please make sure your name only uses alphanumeric characters or spaces",
                    DiscordColor.Red);

                await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var textChannelName = name.Trim().ToLower().Replace(" ", "-");

            if (ctx.Guild.Channels.Any(x => x.Value.Name == name))
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "Oops, a voice or text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    DiscordColor.Orange);
                await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            if (ctx.Guild.Channels.Any(x => x.Value.Name == textChannelName))
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "Oops, a text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    DiscordColor.Orange);
                await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            if (ctx.Guild.Roles.Any(x => x.Value.Name == name))
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "Oops, a role exists with that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    DiscordColor.Orange);
                await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var textChannel = await ctx.Guild.CreateTextChannelAsync(textChannelName).ConfigureAwait(false);
            var voiceChannel = await ctx.Guild.CreateVoiceChannelAsync(name.Trim()).ConfigureAwait(false);

            var newRole = await ctx.Guild.CreateRoleAsync(name.Trim(), Permissions.None, DiscordColor.Blurple, false, false).ConfigureAwait(false);
            var everyoneRole =
                ctx.Guild.GetRole(ctx.Guild.Roles.First(x => x.Value.Name == "@everyone").Value.Id);

            await textChannel.AddOverwriteAsync(newRole,
                Permissions.AccessChannels).ConfigureAwait(false);
            await textChannel.AddOverwriteAsync(everyoneRole,
                deny: Permissions.AccessChannels).ConfigureAwait(false);
            await textChannel.AddOverwriteAsync(ctx.Member,
                Permissions.AccessChannels).ConfigureAwait(false);

            var pair = new ChannelPair
            {
                VoiceChannelGuid = voiceChannel.Id.ToString(),
                RoleGuid = newRole.Id.ToString(),
                TextChannelGuid = textChannel.Id.ToString(),
                GuildGuid = ctx.Guild.Id.ToString(),
                CreationDate =
                    DateTime.UtcNow.ToString(CultureInfo.CurrentCulture),
                Creator = ctx.User.Id.ToString()
            };
            await DynamoSystem.PutItemAsync(pair).ConfigureAwait(false);

            embed = EmbedHandler.GenerateEmbedResponse(
                "I made your Channel Pair, Yay! Order them wherever you like \uD83D\uDE42\r\n" +
                "Make sure to not rename either of the channels or the role they are associated with",
                DiscordColor.Green);

            await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
        }

        /// <exception cref="T:System.ArgumentNullException"><paramref name="oldValue">oldValue</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="oldValue">oldValue</paramref> is the empty string (&amp;
        ///     quot;&amp;quot;).
        /// </exception>
        [Command("remove")]
        [Aliases("r")]
        [Description("Remove a channel pair from the server (or any of it's remains if it's been partially deleted)")]
        [RequirePermissions(Permissions.ManageChannels)]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [UsedImplicitly]
        public async Task RemoveChannelPairAsync(CommandContext ctx, [RemainingText] [NotNull] string name)
        {
            DiscordEmbed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters \uD83D\uDE2E, that won't exist silly :P.\r\n" +
                    "\r\n" +
                    "It should use only alphanumeric characters and spaces.",
                    DiscordColor.Red);

                await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
                return;
            }

            var voiceChannelResults = ctx.Guild.Channels.Where(x => x.Value.Name == name).ToList();
            if (voiceChannelResults.Count > 1)
            {
                embed = EmbedHandler.GenerateEmbedResponse(
                    "Oh no! I found multiple paired voice channels with that name\r\n" +
                    "Please try renaming the one you meant to something unique and try again");
                await ctx.RespondAsync("", false, embed);
                return;
            }

            var pairToRemove = await DynamoSystem.GetItemAsync<ChannelPair>(voiceChannelResults[0].Value.Id);

            await ctx.Guild.Channels.Single(x => x.Value.Id.ToString() == pairToRemove?.TextChannelGuid).Value
                .DeleteAsync();
            await ctx.Guild.Channels.Single(x => x.Value.Id.ToString() == pairToRemove?.VoiceChannelGuid).Value
                .DeleteAsync();
            await ctx.Guild.Roles.Single(x => x.Value.Id.ToString() == pairToRemove?.RoleGuid).Value
                .DeleteAsync();

            await DynamoSystem.DeleteItemAsync(pairToRemove);

            embed = EmbedHandler.GenerateEmbedResponse(
                "I packed up the channel pair you gave me and sent it on it's way, so long! \uD83D\uDE22",
                DiscordColor.Green);

            await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
        }

        [Command("removerecord")]
        [Aliases("rr")]
        [Description("Remove a channel pair record from the database")]
        [RequireOwner]
        [UsedImplicitly]
        public async Task RemoveChannelPairRecordAsync(CommandContext ctx, [NotNull] string guid)
        {
            await DynamoSystem.DeleteItemAsync<ChannelPair>(guid);

            var embed = EmbedHandler.GenerateEmbedResponse(
                "I threw out my info on the channel pair you gave me, my feature will no longer work on it! \uD83D\uDE22",
                DiscordColor.Green);

            await ctx.RespondAsync("", false, embed).ConfigureAwait(false);
        }
    }
}