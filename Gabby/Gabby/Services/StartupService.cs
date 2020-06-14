namespace Gabby.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Gabby.Data;
    using Gabby.Handlers;
    using Gabby.Models;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;

    public sealed class StartupService
    {
        private readonly CommandsNextExtension _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordClient _discord;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordClient discord,
            IConfigurationRoot config)
        {
            this._provider = provider;
            this._config = config;
            this._discord = discord;
            this._commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                Services = provider,
                StringPrefixes = new[] {config["Prefix"]},
                EnableDefaultHelp = false
            });

            this._discord.Ready += this.OnConnected;
            this._discord.GuildCreated += OnJoiningGuild;
            this._discord.GuildDeleted += OnLeavingGuild;
            this._discord.GuildUpdated += OnGuildUpdate;
        }

        /// <exception cref="T:System.Exception">
        ///     Please enter your bot's token into the `_config.yml` file found in the
        ///     applications root directory.
        /// </exception>
        internal async Task StartAsync(CommandHandler handler)
        {
            // var discordToken = this._config["Tokens:Discord"]; // Get the discord token from the config file
            // if (string.IsNullOrWhiteSpace(discordToken))
            //     throw new Exception(
            //         "Please enter your bot's token into the `_config.yml` file found in the applications root directory.");
            handler.PopulateCommands();
            await this._discord.ConnectAsync(new DiscordActivity("with all my friends", ActivityType.Playing)).ConfigureAwait(false); // Login to discord
        }

        private async Task OnConnected(ReadyEventArgs readyEventArgs)
        {
            var recordedGuilds = await DynamoSystem.ScanItemAsync<GuildInfo>();
            var guildsToUpdate = new List<GuildInfo>();
            foreach (var rGuild in recordedGuilds)
            {
                var match = this._discord.Guilds.SingleOrDefault(x => x.Value.Id.ToString() == rGuild.GuildGuid).Value;
                if (match == null)
                {
                    await DynamoSystem.DeleteItemAsync(rGuild);
                    continue;
                }

                rGuild.GuildName = match.Name;
                guildsToUpdate.Add(rGuild);
            }

            foreach (var uGuild in guildsToUpdate) await DynamoSystem.UpdateItemAsync(uGuild);
        }

        private static async Task OnJoiningGuild([NotNull] GuildCreateEventArgs arg)
        {
            var item = new GuildInfo
            {
                GuildGuid = arg.Guild.Id.ToString(),
                GuildName = arg.Guild.Name
            };

            await DynamoSystem.PutItemAsync(item).ConfigureAwait(false);

            var embed = EmbedHandler.GenerateEmbedResponse(
                "Hey friends! Nice to meet you!\r\n" +
                "Type `!help` to get to know me more and find out what I can do!");
            await arg.Guild.GetDefaultChannel().SendMessageAsync("", false, embed);
        }

        private async Task OnLeavingGuild([NotNull] GuildDeleteEventArgs arg)
        {
            var guild = new GuildInfo
            {
                GuildGuid = arg.Guild.Id.ToString(),
                GuildName = arg.Guild.Name
            };

            await DynamoSystem.DeleteItemAsync(guild);
        }

        private async Task OnGuildUpdate([NotNull] GuildUpdateEventArgs arg)
        {
            var guildInfo = await DynamoSystem.GetItemAsync<GuildInfo>(arg.GuildBefore.Id);

            if (guildInfo != null)
            {
                guildInfo.GuildName = arg.GuildAfter.Name;

                await DynamoSystem.UpdateItemAsync(guildInfo);
            }
        }
    }
}