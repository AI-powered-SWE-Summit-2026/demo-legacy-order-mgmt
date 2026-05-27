using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace LegacyOrderMgmt.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Legacy: WebHost.CreateDefaultBuilder — replaced by WebApplication.CreateBuilder in .NET 6+
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
