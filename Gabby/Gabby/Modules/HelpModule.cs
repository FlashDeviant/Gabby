namespace Gabby.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;

    [Name("Help")]
    [Group("help")]
    public sealed class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
        private readonly CommandService _service;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            this._service = service;
            this._config = config;
        }

        [Command]
        [UsedImplicitly]
        public async Task HelpAsync()
        {
            var prefix = this._config["prefix"];
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = "Hiya, my name is Gabby! I'm here to help make talking to everybody even more fun!\r\n" +
                              "\r\n" +
                              "I'm really good at making channel pairs. It's a special pair of channels, one text and one voice. The text one stays hidden until you join the voice channel and goes away when you leave, keeping your server nice and clean.\r\n" +
                              "\r\n" +
                              "I will make sure the channels appear when they need to when I'm online, you can also ask me to make new ones or send them away.\r\n" +
                              "These are the commands you can use:",
                ImageUrl = "https://i.imgur.com/SXqMEuM.png"
            };

            foreach (var module in this._service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(this.Context);
                    if (!result.IsSuccess) continue;

                    description += $"{prefix}{cmd.Aliases.First()}";
                    description = cmd.Parameters.Aggregate(description,
                        (current, parameterInfo) => current + $" <{parameterInfo.Name}>");

                    description += "\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
            }

            await this.ReplyAsync("", false, builder.Build());
        }

        [Command]
        [UsedImplicitly]
        public async Task HelpAsync(string command)
        {
            var result = this._service.Search(this.Context, command);

            if (!result.IsSuccess)
            {
                await this.ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            //var prefix = _config["prefix"];
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var cmd in result.Commands.Select(match => match.Command))
                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });

            await this.ReplyAsync("", false, builder.Build());
        }
    }
}