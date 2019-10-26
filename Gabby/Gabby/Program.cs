using System.Threading.Tasks;

namespace Gabby
{
    class Program
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}