using Amazon.DynamoDBv2.DataModel;

namespace Gabby.Models
{
    [DynamoDBTable("DJChannel")]
    public class DJChannel
    {
        [DynamoDBHashKey] public string GuildGuid { get; set; } //ulong

        public string ChannelId { get; set; }
    }
}
