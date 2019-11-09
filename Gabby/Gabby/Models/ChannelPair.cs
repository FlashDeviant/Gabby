namespace Gabby.Models
{
    using Amazon.DynamoDBv2.DataModel;

    internal sealed class ChannelPair
    {
        [DynamoDBHashKey] public string VoiceChannelGuid { get; set; }

        public string RoleGuid { get; set; }

        public string TextChannelGuid { get; set; }

        public string GuildGuid { get; set; } //ulong

        public string CreationDate { get; set; } //DateTime

        public string Creator { get; set; } //ulong
    }
}