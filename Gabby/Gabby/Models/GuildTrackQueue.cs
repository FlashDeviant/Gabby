using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace Gabby.Models
{
    public class GuildTrackQueue
    {
        public List<QueuedItem> QueuedItems;
        public ulong GuildId;

        public GuildTrackQueue(ulong guildId)
        {
            GuildId = guildId;
            QueuedItems = new List<QueuedItem>();
        }
    }
}
