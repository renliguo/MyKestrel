using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Demo2
{
    public class Program
    {
        public static void Main(string[] args)
        {

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().UseUrls("http://example.com:5000");
            //正确方法
            //.UseKestrel(options => { options.ListenAnyIP(0); });
            //.UseKestrel((context, options) => { options.ListenAnyIP(0); });
            //.ConfigureKestrel(options => { options.ListenAnyIP(0); });

            //错误写法: ListenLocalhost(0) 及 UseUrls("http://localhost:0");
        }
    }
}
