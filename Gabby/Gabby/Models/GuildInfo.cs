namespace Gabby.Models
{
    using Amazon.DynamoDBv2.DataModel;

    [DynamoDBTable("GuildInfo")]
    internal sealed class GuildInfo
    {
        [DynamoDBHashKey] public string GuildGuid { get; set; } //ulong

        public string GuildName { get; set; }
    }
}