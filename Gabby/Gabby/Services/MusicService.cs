using System;
using System.Linq;
using Gabby.Models;

namespace Gabby.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Discord;
    using Discord.WebSocket;
    using Gabby.Handlers;
    using JetBrains.Annotations;
    using Victoria;
    using Victoria.EventArgs;

    public sealed class MusicService
    {
        private readonly LavaNode _lavaNode;
        private readonly LoggingService _logger;

        internal readonly HashSet<ulong> VoteQueue;

        public List<GuildTrackQueue> MusicTrackQueues;

        public MusicService([NotNull] DiscordSocketClient socketClient, LavaNode lavaNode, LoggingService logger)
        {
            socketClient.Ready += this.OnReady;
            MusicTrackQueues = new List<GuildTrackQueue>();
            this._lavaNode = lavaNode;
            this._logger = logger;
            this._lavaNode.OnLog += this.OnLog;
            this._lavaNode.OnPlayerUpdated += this.OnPlayerUpdated;
            this._lavaNode.OnStatsReceived += this.OnStatsReceived;
            this._lavaNode.OnTrackEnded += this.OnTrackEnded;
            this._lavaNode.OnTrackException += this.OnTrackException;
            this._lavaNode.OnTrackStuck += this.OnTrackStuck;
            this._lavaNode.OnWebSocketClosed += this.OnWebSocketClosed;

            this.VoteQueue = new HashSet<ulong>();
        }

        private Task OnLog(LogMessage arg)
        {
            // this._logger.Log(arg.Severity.Convert(), arg.Exception, arg.Message);
            this._logger.LogMessageAsync(arg);
            return Task.CompletedTask;
        }

        private Task OnPlayerUpdated(PlayerUpdateEventArgs arg)
        {
            this._logger.LogDebug($"Player update received for {arg.Player.VoiceChannel.Name}.");
            return Task.CompletedTask;
        }

        private Task OnStatsReceived([NotNull] StatsEventArgs arg)
        {
            this._logger.LogDebug($"Lavalink Uptime {arg.Uptime}.");
            return Task.CompletedTask;
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            var player = args.Player;

            MusicTrackQueues.Single(x => x.GuildId == args.Player.VoiceChannel.GuildId).QueuedItems
                .RemoveAll(x => x.Track.Id == args.Track.Id);

            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("", false, EmbedHandler.GenerateEmbedResponse("No more tracks to play."));
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("", false, EmbedHandler.GenerateEmbedResponse("Next item in queue is not a track."));
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync("", false, EmbedHandler.GenerateEmbedResponse($"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}"));
            var artwork = await track.FetchArtworkAsync();

            var requestingUser = MusicTrackQueues.Single(x => x.GuildId == args.Player.VoiceChannel.GuildId).QueuedItems.First().RequestingUser;

            var embed = new EmbedBuilder
                {
                    Title = track.Title,
                    Description = track.Author,
                    ThumbnailUrl = artwork,
                    Url = track.Url,
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Next Up!"
                    },
                    Footer = new EmbedFooterBuilder
                    {
                        Text =
                            $"This track was requested by {requestingUser.Username}#{requestingUser.Discriminator}",
                        IconUrl = requestingUser.GetAvatarUrl()
                    }
                }
                .AddField("Duration", $@"{track.Duration:mm\:ss}");
        }

        private Task OnTrackException(TrackExceptionEventArgs arg)
        {
            this._logger.LogCritical($"Track exception received for {arg.Track.Title}.", arg.ErrorMessage);
            return Task.CompletedTask;
        }

        private Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            this._logger.LogError($"Track stuck received for {arg.Track.Title}.");
            return Task.CompletedTask;
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            this._logger.LogCritical($"Discord WebSocket connection closed with following reason: {arg.Reason}", arg.Code.ToString());
            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            await this._lavaNode.ConnectAsync();
        }
    }
}