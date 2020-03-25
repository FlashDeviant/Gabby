namespace Gabby.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Gabby.Handlers;
    using Gabby.Services;
    using JetBrains.Annotations;
    using Victoria;
    using Victoria.Enums;
    using Victoria.Responses.Rest;

    [UsedImplicitly]
    public sealed class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly MusicService _musicService;
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        public MusicModule(LavaNode lavaNode, MusicService musicService)
        {
            this._lavaNode = lavaNode;
            this._musicService = musicService;
        }

        [UsedImplicitly]
        [Command("Join")]
        [Summary("The bot will join the channel the user is connected to")]
        public async Task JoinAsync()
        {
            if (this._lavaNode.HasPlayer(this.Context.Guild))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm already connected to a voice channel!", Color.Orange));
                return;
            }

            var voiceState = this.Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("You must be connected to a voice channel!", Color.Red));
                return;
            }

            await this._lavaNode.JoinAsync(voiceState.VoiceChannel, this.Context.Channel as ITextChannel);
            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Joined {voiceState.VoiceChannel.Name}"));
        }

        [UsedImplicitly]
        [Command("Leave")]
        [Summary("The bot will leave the channel it is currently connected to")]
        public async Task LeaveAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to any voice channels"));
                return;
            }

            var voiceChannel = ((IVoiceState) this.Context.User).VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Not sure which voice channel to disconnect from."));
                return;
            }

            await this._lavaNode.LeaveAsync(voiceChannel);
            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"I've left {voiceChannel.Name}"));
        }

        private async Task<SearchResponse> ValidationCheck([CanBeNull] string query, SearchType? type = null)
        {
            SearchResponse searchResponse;

            if (string.IsNullOrWhiteSpace(query)) await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Please provide search terms."));

            if (!this._lavaNode.HasPlayer(this.Context.Guild))
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));


            searchResponse = type switch
            {
                SearchType.Youtube => await this._lavaNode.SearchYouTubeAsync(query),
                SearchType.Soundcloud => await this._lavaNode.SearchSoundCloudAsync(query),
                null => await this._lavaNode.SearchAsync(query),
            };

            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"I wasn't able to find anything for `{query}`."));

            return searchResponse;
        }

        private async Task EnqueueTracks(SearchResponse searchResponse, LavaPlayer player)
        {
            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                foreach (var track in searchResponse.Tracks) player.Queue.Enqueue(track);

                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Enqueued {searchResponse.Tracks.Count} tracks."));
            }
            else
            {
                var track = searchResponse.Tracks[0];
                player.Queue.Enqueue(track);
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Enqueued: {track.Title}"));
            }
        }

        private async Task EnqueueSingleTrack(SearchResponse searchResponse, LavaPlayer player)
        {
            var track = searchResponse.Tracks[0];

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                for (var i = 0; i < searchResponse.Tracks.Count; i++)
                {
                    if (i == 0)
                    {
                        await player.PlayAsync(track);
                        await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Now Playing: {track.Title}"));
                    }
                    else
                    {
                        player.Queue.Enqueue(searchResponse.Tracks[i]);
                    }
                }

                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Enqueued {searchResponse.Tracks.Count} tracks."));
            }
            else
            {
                await player.PlayAsync(track);
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Now Playing: {track.Title}"));
            }
        }

        [UsedImplicitly]
        [Command("Play")]
        [Summary("The bot will play/queue the provided music link")]
        public async Task PlayAsync([Remainder] [CanBeNull] string query)
        {
            var searchResponse = await this.ValidationCheck(query);

            var player = this._lavaNode.GetPlayer(this.Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await this.EnqueueTracks(searchResponse, player);
            }
            else
            {
                await this.EnqueueSingleTrack(searchResponse, player);
            }
        }

        public enum SearchType
        {
            Youtube,
            Soundcloud
        }

        [UsedImplicitly]
        [Command("Search")]
        [Summary("The bot will look for a YouTube or Soundcloud song to play matching your search")]
        public async Task SearchAsync(SearchType type, [Remainder] [CanBeNull] string query)
        {
            var searchResponse = await this.ValidationCheck(query, type);

            var player = this._lavaNode.GetPlayer(this.Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await this.EnqueueTracks(searchResponse, player);
            }
            else
            {
                await this.EnqueueSingleTrack(searchResponse, player);
            }
        }

        [UsedImplicitly]
        [Command("Pause")]
        [Summary("The bot will pause the currently playing track")]
        public async Task PauseAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I cannot pause when I'm not playing anything"));
                return;
            }

            await player.PauseAsync();
            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Paused: {player.Track.Title}"));
        }

        [UsedImplicitly]
        [Command("Resume")]
        [Summary("The bot will resume the currently playing track")]
        public async Task ResumeAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I cannot resume when I'm not playing anything"));
                return;
            }

            await player.ResumeAsync();
            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Resumed: {player.Track.Title}"));
        }

        [UsedImplicitly]
        [Command("Stop")]
        [Summary("The bot will stop playing the current track and clear the queue")]
        public async Task StopAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I can't stop the stopped forced."));
                return;
            }


            await player.StopAsync();
            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("No longer playing anything."));
        }

        [UsedImplicitly]
        [Command("Skip")]
        [Summary("The bot will skip the currently playing track")]
        public async Task SkipAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I can't skip when nothing is playing."));
                return;
            }

            var voiceChannelUsers = (player.VoiceChannel as SocketVoiceChannel).Users.Where(x => !x.IsBot).ToArray();
            if (this._musicService.VoteQueue.Contains(this.Context.User.Id))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("You can't vote again."));
                return;
            }

            this._musicService.VoteQueue.Add(this.Context.User.Id);
            var percentage = this._musicService.VoteQueue.Count / voiceChannelUsers.Length * 100;
            if (percentage < 85)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("You need more than 85% votes to skip this song."));
                return;
            }

            try
            {
                var oldTrack = player.Track;
                var currenTrack = await player.SkipAsync();
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"Skipped: {oldTrack.Title}\nNow Playing: {currenTrack.Title}"));
            }
            catch (Exception exception)
            {
                await this.ReplyAsync(exception.Message);
            }
        }

        [UsedImplicitly]
        [Command("Seek")]
        [Summary("The bot will seek to the chosen time of the currently playing track")]
        public async Task SeekAsync(TimeSpan timeSpan)
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I can't seek when nothing is playing."));
                return;
            }

            try
            {
                await player.SeekAsync(timeSpan);
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"I've seeked `{player.Track.Title}` to {timeSpan}."));
            }
            catch (Exception exception)
            {
                await this.ReplyAsync(exception.Message);
            }
        }

        [UsedImplicitly]
        [Command("Volume")]
        [Summary("The bot will adjust the volume to the selected amount")]
        public async Task VolumeAsync(ushort volume)
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            try
            {
                await player.UpdateVolumeAsync(volume);
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"I've changed the player volume to {volume}."));
            }
            catch (Exception exception)
            {
                await this.ReplyAsync(exception.Message);
            }
        }

        [UsedImplicitly]
        [Command("NowPlaying")]
        [Alias("Np")]
        [Summary("The bot will display whats playing")]
        public async Task NowPlayingAsync()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I'm not playing any tracks."));
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var embed = new EmbedBuilder
                {
                    Title = $"{track.Author} - {track.Title}",
                    ThumbnailUrl = artwork,
                    Url = track.Url
                }
                .AddField("Duration", track.Duration.ToString("MM:ss"))
                .AddField("Position", track.Position.ToString("MM:ss"));

            await this.ReplyAsync(embed: embed.Build());
        }

        [UsedImplicitly]
        [Command("Genius", RunMode = RunMode.Async)]
        [Summary("The bot will show lyrics for the current song from Genius")]
        public async Task ShowGeniusLyrics()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I'm not playing any tracks."));
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"No lyrics found for {player.Track.Title}"));
                return;
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
            {
                if (Range.Contains(stringBuilder.Length))
                {
                    await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"```{stringBuilder}```"));
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }

            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"```{stringBuilder}```"));
        }

        [UsedImplicitly]
        [Command("OVH", RunMode = RunMode.Async)]
        [Summary("The bot will show lyrics for the current song from OVH")]
        public async Task ShowOVHLyrics()
        {
            if (!this._lavaNode.TryGetPlayer(this.Context.Guild, out var player))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("I'm not connected to a voice channel."));
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse("Woaaah there, I'm not playing any tracks."));
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromOVHAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"No lyrics found for {player.Track.Title}"));
                return;
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
                if (Range.Contains(stringBuilder.Length))
                {
                    await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"```{stringBuilder}```"));
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }

            await this.ReplyAsync("", false, EmbedHandler.GenerateEmbedResponse($"```{stringBuilder}```"));
        }
    }
}