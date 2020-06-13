namespace Gabby.Handlers
{
    using System;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;

    [UsedImplicitly]
    public sealed class CommandHandler
    {
        private readonly CommandsNextExtension _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordClient _discord;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordClient discord,
            CommandsNextExtension commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            this._discord = discord;
            this._commands = commands;
            this._config = config;

            this._discord.MessageCreated += this.OnMessageCreatedAsync;
            this._commands.CommandErrored += this.OnCommandErrored;
            this._commands.CommandExecuted += this.OnCommandExecuted;
        }

        private async Task OnCommandExecuted([NotNull] CommandExecutionEventArgs e)
        {
            await e.Context.Message.DeleteAsync();
        }

        private async Task OnCommandErrored([NotNull] CommandErrorEventArgs e)
        {
            var embed = EmbedHandler.GenerateEmbedResponse("Uh oh, something went wrong:\r\n" + $"{e.Exception.Message}\r\n\r\nPlease try using '{this._config["Prefix"]}help <command>' to find out how to use this command",
                DiscordColor.Red);
            await e.Context.Channel.SendMessageAsync("", false, embed).ConfigureAwait(false);
        }

        private async Task OnMessageCreatedAsync([NotNull] MessageCreateEventArgs msg)
        {
            if (msg.Author.Id == this._discord.CurrentUser.Id) return; // Ignore self when checking commands

            if (msg.Message.GetStringPrefixLength(this._config["Prefix"]) == 0 ||
                msg.Message.GetMentionPrefixLength(this._discord.CurrentUser) == 0)
            {
                var context = this._commands.CreateContext(msg.Message, this._config["Prefix"],
                    this._commands.FindCommand(msg.Message.ToString(), out var args), args); // Create the command context
                await this._commands.ExecuteCommandAsync(context)
                    .ConfigureAwait(false); // Execute the command
            }
        }
    }
}