namespace Gabby.Handlers
{
    using System.Text.RegularExpressions;
    using DSharpPlus.Entities;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class EmbedHandler
    {
        internal static DiscordEmbed GenerateEmbedResponse(string message)
        {
            var builder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(114, 137, 218),
                Description = message
            };

            return builder.Build();
        }

        internal static DiscordEmbed GenerateEmbedResponse(string message, DiscordColor color)
        {
            var builder = new DiscordEmbedBuilder
            {
                Color = color,
                Description = message
            };

            return builder.Build();
        }

        internal static DiscordEmbed GenerateYouTubeMediaEmbedResponse(string message, [NotNull] string youtubeVideoLink)
        {
            var vidRegex = new Regex(@"(.*?)(^|\/|v=)([a-z0-9_-]{11})(.*)?", RegexOptions.IgnoreCase);
            // const string vidRegex = @"/^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/";
            // const string listRegex = @"list=([a-zA-Z0-9\-\_]+)&?";
            var regex = vidRegex.Match(youtubeVideoLink);
            var youtubeVideoId = regex.Groups[3].ToString();

            var builder = new DiscordEmbedBuilder
            {
                Description = $"\uD83C\uDFB5 {message}",
                ImageUrl = $"https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg"
            };

            return builder.Build();
        }
    }
}