namespace Gabby.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using JetBrains.Annotations;

    public sealed class LoggingService
    {
        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        // ReSharper disable once SuggestBaseTypeForParameter
        public LoggingService([NotNull] DiscordSocketClient discord, [NotNull] CommandService commands)
        {
            this.LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            discord.Log += this.OnLogAsync;
            commands.Log += this.OnLogAsync;
        }

        private string LogDirectory { get; }
        private string LogFile => Path.Combine(this.LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(this.LogDirectory)) // Create the log directory if it doesn't exist
                Directory.CreateDirectory(this.LogDirectory);
            if (!File.Exists(this.LogFile)) // Create today's log file if it doesn't exist
                File.Create(this.LogFile).Dispose();

            var logText =
                $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(this.LogFile, logText + "\n"); // Write the log text to a file

            return Console.Out.WriteLineAsync(logText); // Write the log text to the console
        }
    }
}