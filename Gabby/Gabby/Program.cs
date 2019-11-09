namespace Gabby
{
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    internal static class Program
    {
        [NotNull]
        public static Task Main(string[] args)
        {
            return Startup.RunAsync(args);
        }
    }
}