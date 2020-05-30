using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Victoria;

namespace Gabby.Models
{
    public class QueuedItem
    {
        public LavaTrack Track;
        public SocketUser RequestingUser;

        public QueuedItem(LavaTrack lavaTrack, SocketUser socketUser)
        {
            Track = lavaTrack;
            RequestingUser = socketUser;
        }
    }
}
