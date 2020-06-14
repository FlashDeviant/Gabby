namespace Gabby.Services
{
    using System.Collections.Generic;
    using DSharpPlus;
    using Gabby.Models;
    using Lavalink4NET;

    public class MusicService
    {
        public List<DJChannel> DJChannels;

        private DiscordClient _discord;
        private IAudioService _audio;

        public MusicService(DiscordClient discord, IAudioService audio)
        {
            this._discord = discord;
            this._audio = audio;
            this.DJChannels = new List<DJChannel>();

            this._discord.Ready += args => this._audio.InitializeAsync();
        }
    }
}