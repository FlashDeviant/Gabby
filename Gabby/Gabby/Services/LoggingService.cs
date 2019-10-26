using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace Gabby.Services
{
    public sealed class LoggingService
    {
        private string LogDirectory { get; }
        private string LogFile => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        // ReSharper disable once SuggestBaseTypeForParameter
        public LoggingService([NotNull] DiscordSocketClient discord, [NotNull] CommandService commands)
        {
            LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

            discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(LogDirectory)) // Create the log directory if it doesn't exist
                Directory.CreateDirectory(LogDirectory);
            if (!File.Exists(LogFile)) // Create today's log file if it doesn't exist
                File.Create(LogFile).Dispose();

            var logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(LogFile, logText + "\n"); // Write the log text to a file

            return Console.Out.WriteLineAsync(logText); // Write the log text to the console
        }
    }
}