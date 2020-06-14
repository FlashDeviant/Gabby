namespace Gabby
{
    using System;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using DSharpPlus;
    using Gabby.Handlers;
    using Gabby.Services;
    using Lavalink4NET;
    using Lavalink4NET.DSharpPlus;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class Startup
    {
        internal static IConfigurationRoot StaticConfiguration;

        // ReSharper disable once UnusedParameter.Local
        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder() // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory) // Specify the default location for the config file
                .AddYamlFile("_config.yml"); // Add this (yaml encoded) file to the configuration
            this.Configuration = builder.Build(); // Build the configuration
            StaticConfiguration = this.Configuration;
        }

        private IConfigurationRoot Configuration { get; }

        internal static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        private async Task RunAsync()
        {
            var services = new ServiceCollection(); // Create a new instance of a service collection
            this.ConfigureServices(services);

            var provider = services.BuildServiceProvider(); // Build the service provider

            var commandHandler = provider.GetRequiredService<CommandHandler>(); // Start the command handler service

            provider.GetRequiredService<PairHandler>();
            provider.GetRequiredService<GuildHandler>();
            provider.GetRequiredService<MusicService>();

            await provider.GetRequiredService<StartupService>().StartAsync(commandHandler); // Start the startup service
            await Task.Delay(-1); // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton(new DiscordClient(new DiscordConfiguration()
                {
                    // Add discord to the collection
                    LogLevel = Enum.Parse<LogLevel>(this.Configuration["Client:LogLevel"], true), // Tell the logger to give Verbose amount of info
                    MessageCacheSize = Convert.ToInt32(this.Configuration["Client:MessageCacheSize"]), // Cache 1,000 messages per channel
                    UseInternalLogHandler = true,
                    Token = StaticConfiguration["Tokens:Discord"],
                    TokenType = TokenType.Bot
                }))
                .AddSingleton<CommandHandler>() // Add the command handler to the collection
                .AddSingleton<PairHandler>()
                .AddSingleton<GuildHandler>()
                .AddSingleton<MusicService>()
                .AddSingleton<StartupService>() // Add startup service to the collection
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton(new LavalinkNodeOptions
                {
                    RestUri = this.Configuration["LavaLink:RestUri"],
                    WebSocketUri = this.Configuration["LavaLink:WebSocketUri"],
                    Password = this.Configuration["LavaLink:Password"]
                })
                .AddSingleton(this.Configuration); // Add the configuration to the collection
        }
    }
}