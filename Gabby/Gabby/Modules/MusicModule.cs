namespace Gabby.Modules
{
    using System;
    using System.Threading.Tasks;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Gabby.Handlers;
    using Gabby.Services;
    using JetBrains.Annotations;
    using Lavalink4NET;
    using Lavalink4NET.Rest;

    public sealed class MusicModule : BaseCommandModule
    {
        private readonly IAudioService _audio;
        private readonly MusicService _service;

        public MusicModule(IAudioService audio, MusicService service)
        {
            this._audio = audio;
            this._service = service;
        }

        [Command("set")]
        [Description("Sets the DJ Channel up, switching the bot into DJ Mode")]
        public async Task Set(CommandContext ctx)
        {
            var messages = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id);
            if (messages.Count > 0)
            {
                // ctx.RespondAsync(embed: EmbedHandler.GenerateEmbedResponse())
            }
        }

        [Command("unset")]
        [Description("Unsets the currently established DJ Channel in the server, returns bot to command mode")]
        public async Task Unset(CommandContext ctx)
        {

        }

        [Command("join")]
        [Description("Joins the executors current voice channel")]
        [UsedImplicitly]
        public async Task Join(CommandContext ctx)
        {
            var _ = this._audio.GetPlayer(ctx.Guild.Id)
                    ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await this.SetVolume(ctx, 10);
        }

        [Command("leave")]
        [Description("Leave the voice channel I'm connected to")]
        [UsedImplicitly]
        public async Task Leave(CommandContext ctx)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.DisconnectAsync();
        }

        [Command("play")]
        [Description("Plays a music track into voice channel")]
        [UsedImplicitly]
        public async Task Play(CommandContext ctx, string link)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            var myTrack = await this._audio.GetTrackAsync(link, SearchMode.YouTube);
            await player.PlayAsync(myTrack);
        }

        [Command("stop")]
        [Description("Stops any music playback and destroys the queue")]
        [UsedImplicitly]
        public async Task Stop(CommandContext ctx)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.StopAsync(false);
        }

        [Command("pause")]
        [Description("Pauses the current track")]
        [UsedImplicitly]
        public async Task Pause(CommandContext ctx)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.PauseAsync();
        }

        [Command("resume")]
        [Description("Resumes the current track")]
        [UsedImplicitly]
        public async Task Resume(CommandContext ctx)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.ResumeAsync();
        }

        [Command("replay")]
        [Description("Replays the current track")]
        [UsedImplicitly]
        public async Task Replay(CommandContext ctx)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.ReplayAsync();
        }

        [Command("seek")]
        [Description("Sets the playback of the current track to the desired time")]
        [UsedImplicitly]
        public async Task Seek(CommandContext ctx, TimeSpan position)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.SeekPositionAsync(position);
        }

        [Command("volume")]
        [Description("Sets the volume of the music playback")]
        [UsedImplicitly]
        public async Task SetVolume(CommandContext ctx, float volume)
        {
            var player = this._audio.GetPlayer(ctx.Guild.Id)
                         ?? await this._audio.JoinAsync(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id);
            await player.SetVolumeAsync(volume / 100);
        }
    }
}