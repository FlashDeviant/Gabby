using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;

namespace Gabby.Modules
{
    [Name("Example")]
    [UsedImplicitly]
    public sealed class ChannelModule : ModuleBase<SocketCommandContext>
    {
        [Command("createchannelpair")]
        [Alias("ccp")]
        [Summary("Create a text/voice channel pair with a role to control whether the text channel is visible")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task CreateChannelPair([Remainder] [NotNull] string text)
        {
            if (!text.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                await Context.Channel.SendMessageAsync(
                    "The channel name you gave me contains funny characters.\r\nPlease make sure your name only uses alphanumeric characters or spaces");
                return;
            }

            var textChannelName = text.Trim().ToLower().Replace(" ", "-");

            var textChannel = await Context.Guild.CreateTextChannelAsync(textChannelName);
            await Context.Guild.CreateVoiceChannelAsync(text.Trim());

            var newRole = await Context.Guild.CreateRoleAsync(text.Trim());
            var everyoneRole = Context.Guild.GetRole(Context.Guild.Roles.First(x => x.Name == "@everyone").Id);

            await textChannel.AddPermissionOverwriteAsync(newRole,
                new OverwritePermissions(viewChannel: PermValue.Allow));
            await textChannel.AddPermissionOverwriteAsync(everyoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));

            await Context.Channel.SendMessageAsync(
                "I made your Channel Pair, Yay! Order them wherever you like :)\r\nMake sure to not rename either of the channels or the role they are associated with");
        }

        [Command("removechannelpair")]
        [Alias("rcp")]
        [Summary("Create a text/voice channel pair with a role to control whether the text channel is visible")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [UsedImplicitly]
        public async Task RemoveChannelPair([Remainder] [NotNull] string text)
        {
            if (!text.All(c => char.IsLetterOrDigit(c) || c == ' '))
            {
                await Context.Channel.SendMessageAsync(
                    "The channel name you gave me contains funny characters :o, that won't exist silly :P.\r\nIt should use only alphanumeric characters and spaces.");
                return;
            }

            var textChannelName = text.Trim().ToLower().Replace(" ", "-");

            if (!Context.Guild.Channels.Any(x => x.Name == textChannelName) &&
                !Context.Guild.Roles.Any(x => x.Name == text) && !Context.Guild.Roles.Any(x => x.Name == text))
            {
                await Context.Channel.SendMessageAsync(
                    "Oh no! I can't find any channels with that name or any leftovers to remove");
                return;
            }

            var textChannelToDelete = Context.Guild.Channels.SingleOrDefault(x => x.Name == textChannelName);
            var voiceChannelToDelete = Context.Guild.Channels.SingleOrDefault(x => x.Name == text.Trim());
            var roleToDelete = Context.Guild.Roles.SingleOrDefault(x => x.Name == text.Trim());

            textChannelToDelete?.DeleteAsync();
            voiceChannelToDelete?.DeleteAsync();
            roleToDelete?.DeleteAsync();

            await Context.Channel.SendMessageAsync(
                "I packed up the channel pair you gave me and sent it on it's way, so long! :,)");
        }
    }
}