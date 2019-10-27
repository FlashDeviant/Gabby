using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

namespace Gabby.Modules
{
    [Name("Channel Pairs")]
    [UsedImplicitly]
    public sealed class ChannelModule : ModuleBase<SocketCommandContext>
    {
        [Command("createchannelpair")]
        [Alias("ccp")]
        [Summary("Create a text/voice channel pair with a role to control whether the name channel is visible")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task CreateChannelPair([Remainder] [NotNull] string name)
        {
            Embed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters.\r\n" +
                    "\r\n" +
                    "Please make sure your name only uses alphanumeric characters or spaces",
                    Color.Red);

                await ReplyAsync("", false, embed);
                return;
            }

            var textChannelName = name.Trim().ToLower().Replace(" ", "-");

            if (Context.Guild.Channels.Any(x => x.Name == name))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a voice or text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await ReplyAsync("", false, embed);
                return;
            }

            if (Context.Guild.Channels.Any(x => x.Name == textChannelName))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a text channel is already using that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await ReplyAsync("", false, embed);
                return;
            }

            if (Context.Guild.Roles.Any(x => x.Name == name))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oops, a role exists with that name already!\r\n" +
                    "I guess it's pretty popular \uD83D\uDE2E",
                    Color.Orange);
                await ReplyAsync("", false, embed);
                return;
            }

            var textChannel = await Context.Guild.CreateTextChannelAsync(textChannelName);
            await Context.Guild.CreateVoiceChannelAsync(name.Trim());

            var newRole = await Context.Guild.CreateRoleAsync(name.Trim());
            var everyoneRole = Context.Guild.GetRole(Context.Guild.Roles.First(x => x.Name == "@everyone").Id);

            await textChannel.AddPermissionOverwriteAsync(newRole,
                new OverwritePermissions(viewChannel: PermValue.Allow));
            await textChannel.AddPermissionOverwriteAsync(everyoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));

            embed = MessageModule.GenerateEmbedResponse(
                "I made your Channel Pair, Yay! Order them wherever you like \uD83D\uDE42\r\n" +
                "Make sure to not rename either of the channels or the role they are associated with",
                Color.Green);

            await ReplyAsync("", false, embed);
        }

        [Command("removechannelpair")]
        [Alias("rcp")]
        [Summary("Remove a channel pair from the server (or any of it's remains if it's been partially deleted)''")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task RemoveChannelPair([Remainder] [NotNull] string name)
        {
            Embed embed;
            if (!name.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "The channel name you gave me contains funny characters \uD83D\uDE2E, that won't exist silly :P.\r\n" +
                    "\r\n" +
                    "It should use only alphanumeric characters and spaces.",
                    Color.Red);

                await ReplyAsync("", false, embed);
                return;
            }

            var textChannelName = name.Trim().ToLower().Replace(" ", "-");

            var a = Context.Guild.Channels.All(x => x.Name != textChannelName);
            var b = Context.Guild.Channels.All(x => x.Name != name);
            var d = Context.Guild.Roles.All(x => x.Name != name);

            if (Context.Guild.Channels.All(x => x.Name != textChannelName) &&
                Context.Guild.Channels.All(x => x.Name != name) && Context.Guild.Roles.All(x => x.Name != name))
            {
                embed = MessageModule.GenerateEmbedResponse(
                    "Oh no! I can't find any channels with that name or any leftovers to remove",
                    Color.Red);

                await ReplyAsync("", false, embed);
                return;
            }

            var textChannelToDelete = Context.Guild.Channels.SingleOrDefault(x => x.Name == textChannelName);
            var voiceChannelToDelete = Context.Guild.Channels.SingleOrDefault(x => x.Name == name.Trim());
            var roleToDelete = Context.Guild.Roles.SingleOrDefault(x => x.Name == name.Trim());

            textChannelToDelete?.DeleteAsync();
            voiceChannelToDelete?.DeleteAsync();
            roleToDelete?.DeleteAsync();

            embed = MessageModule.GenerateEmbedResponse(
                "I packed up the channel pair you gave me and sent it on it's way, so long! \uD83D\uDE22",
                Color.Green);

            await ReplyAsync("", false, embed);
        }
    }
}