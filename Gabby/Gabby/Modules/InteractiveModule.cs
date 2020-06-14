namespace Gabby.Modules
{
    using System.Threading.Tasks;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using Gabby.Handlers;
    using JetBrains.Annotations;

    public sealed class InteractiveModule : BaseCommandModule
    {
        // private InteractiveService _service;

        // public InteractiveModule(InteractiveService service)
        // {
        //     this._service = service;
        // }

        [UsedImplicitly]
        [Command("page")]
        public Task Page(CommandContext ctx)
        {
            var interact = ctx.Client.GetInteractivity();
            var page1 = new Page
            {
                Embed = EmbedHandler.GenerateYouTubeMediaEmbedResponse(
                    "New Frame PROTEA In Action & Build - Method to Madness",
                    "https://www.youtube.com/watch?v=Y35w8EvlEWo")
            };
            var page2 = new Page
            {
                Embed = EmbedHandler.GenerateYouTubeMediaEmbedResponse("Warframe: New Void Missions & How To Run Them",
                    "https://www.youtube.com/watch?v=kCe5vJLXPtg")
            };

            var emoji = new PaginationEmojis
            {
                Right = DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"),
                Left = DiscordEmoji.FromName(ctx.Client, ":arrow_backward:"),
                SkipLeft = null,
                SkipRight = null,
                Stop = null
            };

            return interact.SendPaginatedMessageAsync(ctx.Channel, ctx.User, new[] {page1, page2}, emoji, PaginationBehaviour.Ignore, PaginationDeletion.DeleteMessage);
        }

        [Command("upmess")]
        public async Task UpdatingMessage(CommandContext ctx)
        {
            var interact = ctx.Client.GetInteractivity();
            var builder = new DiscordEmbedBuilder
            {
                Title = "Hello World!",
                Description = "0"
            };

            var embed = builder.Build();
            var m = await ctx.RespondAsync(embed: embed);
            var bock = true;
            while (bock)
            {
                var resp = await interact.WaitForMessageAsync(x => x.Author.Id == ctx.User.Id);
                await resp.Result.DeleteAsync();
                if (resp.Result.Content == "end") break;

                builder.Description = resp.Result.Content;
                await m.ModifyAsync(embed: builder.Build());
            }
        }
    }
}