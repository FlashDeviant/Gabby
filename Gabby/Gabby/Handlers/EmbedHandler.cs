namespace Gabby.Handlers
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Gabby.Modules;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class EmbedHandler
    {
        internal static Embed GenerateEmbedResponse(string message)
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = message
            };

            return builder.Build();
        }

        internal static Embed GenerateEmbedResponse(string message, Color color)
        {
            var builder = new EmbedBuilder
            {
                Color = color,
                Description = message
            };

            return builder.Build();
        }

        internal static Embed GenerateYouTubeMediaEmbedResponse(string message, [NotNull] string youtubeVideoLink)
        {
            var vidRegex = new Regex(@"(.*?)(^|\/|v=)([a-z0-9_-]{11})(.*)?", RegexOptions.IgnoreCase);
            // const string vidRegex = @"/^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/";
            // const string listRegex = @"list=([a-zA-Z0-9\-\_]+)&?";
            var regex = vidRegex.Match(youtubeVideoLink);
            var youtubeVideoId = regex.Groups[3].ToString();

            var builder = new EmbedBuilder
            {
                Description = $"\uD83C\uDFB5 {message}",
                ImageUrl = $"https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg"
            };

            return builder.Build();
        }
    }
}