namespace Gabby.Modules
{
    using Discord;

    internal static class MessageModule
    {
        public static Embed GenerateEmbedResponse(string message)
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = message
            };

            return builder.Build();
        }

        public static Embed GenerateEmbedResponse(string message, Color color)
        {
            var builder = new EmbedBuilder
            {
                Color = color,
                Description = message
            };

            return builder.Build();
        }
    }
}