namespace Gabby.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Gabby.Modules;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;

    [UsedImplicitly]
    public sealed class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            this._discord = discord;
            this._commands = commands;
            this._config = config;
            this._provider = provider;

            this._discord.MessageReceived += this.OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;
            if (msg.Author.Id == this._discord.CurrentUser.Id) return; // Ignore self when checking commands

            var context = new SocketCommandContext(this._discord, msg); // Create the command context

            var argPos = 0; // Check if the message has a valid command prefix
            if (msg.HasStringPrefix(this._config["Prefix"], ref argPos) ||
                msg.HasMentionPrefix(this._discord.CurrentUser, ref argPos))
            {
                var result = await this._commands.ExecuteAsync(context, argPos, this._provider)
                    .ConfigureAwait(false); // Execute the command

                if (!result.IsSuccess) // If not successful, reply with the error.
                {
                    var embed = MessageModule.GenerateEmbedResponse("Uh oh, something went wrong:\r\n" + $"{result}",
                        Color.Red);
                    await context.Channel.SendMessageAsync("", false, embed).ConfigureAwait(false);
                }
            }
        }
    }
}