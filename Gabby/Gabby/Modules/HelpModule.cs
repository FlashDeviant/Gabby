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
    using Gabby.Handlers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;

    public sealed class HelpModule : BaseCommandModule
    {
        private readonly IConfigurationRoot _config;
        private readonly CommandsNextExtension _service;

        public HelpModule(DiscordClient client, IConfigurationRoot config)
        {
            this._service = client.GetCommandsNext();
            this._config = config;
        }

        [Command]
        [UsedImplicitly]
        public async Task HelpAsync([NotNull] CommandContext ctx)
        {
            var prefix = this._config["prefix"];
            var builder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(114, 137, 218),
                Title = "Hiya, my name is Gabby!",
                Description = "I'm here to help make talking to everybody even more fun!\r\n" +
                              "\r\n" +
                              "I'm really good at making channel pairs. It's a special pair of channels, one text and one voice. The text one stays hidden until you join the voice channel and goes away when you leave, keeping your server nice and clean.\r\n" +
                              "\r\n" +
                              "I will make sure the channels appear when they need to when I'm online, you can also ask me to make new ones or send them away.\r\n" +
                              "These are the commands you can use:",
                ImageUrl = "https://i.imgur.com/SXqMEuM.png"
            };

            foreach (var (moduleName, commands) in this._service.RegisteredCommands)
            {
                var commandGroup = commands as CommandGroup;
                string description = null;

                if (commandGroup != null)
                {
                    if (commandGroup.Aliases.Contains(moduleName))
                        continue; // Exclude aliases as individual modules

                    var result = await commandGroup.RunChecksAsync(ctx, true);
                    if (result.Any()) continue;

                    foreach (var command in commandGroup.Children)
                    {
                        foreach (var overload in command.Overloads)
                        {
                            description += $"{prefix}{command.QualifiedName}";
                            description = overload.Arguments.Aggregate(description,
                                (current, parameterInfo) => current + $" <{parameterInfo.Name}>");
                            description += "\n";
                        }
                    }
                }
                else
                {
                    foreach (var overload in commands.Overloads)
                    {
                        description += $"{prefix}{commands.QualifiedName}";
                        description = overload.Arguments.Aggregate(description,
                            (current, parameterInfo) => current + $" <{parameterInfo.Name}>");
                        description += "\n";
                    }
                }

                var cultInfo = new CultureInfo("en-GB", false).TextInfo;

                if (!string.IsNullOrWhiteSpace(description))
                    builder.AddField($"{cultInfo.ToTitleCase(moduleName.Replace('-', ' '))} Module", description,
                        false);
            }

            await ctx.RespondAsync("", false, builder.Build());
        }


        [Command]
        [UsedImplicitly]
        public async Task HelpAsync([NotNull] CommandContext ctx, [RemainingText] string command)
        {
            var result = this._service.FindCommand(command, out var args);
            var prefix = this._config["prefix"];

            if (result == null)
            {
                await ctx.RespondAsync(embed: EmbedHandler.GenerateEmbedResponse($"Sorry, I couldn't find a command like **{command}**.", DiscordColor.Orange));
                return;
            }

            var builder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(114, 137, 218),
            };

            if (result is CommandGroup group)
            {
                builder.Title = $"I've found {group.Children.Count} sub-command(s) under this command";
                foreach (var child in group.Children)
                    foreach (var overload in child.Overloads)
                        builder.AddField(
                            $"{prefix}{child.QualifiedName} <{string.Join("> <", overload.Arguments.Select(p => p.Name))}>",
                            child.Description);
            }
            else
            {
                builder.Title = $"Help for _\"{result.QualifiedName}\"_";

                foreach (var overload in result.Overloads)
                {
                    builder.AddField("Summary:",
                        $"Summary: {result.Description}", false);
                    builder.AddField("Parameters",
                        $"`{string.Join("`, `", overload.Arguments.Select(p => p.Name))}`", false);
                }
            }

            await ctx.RespondAsync(embed: builder.Build());
        }
    }
}