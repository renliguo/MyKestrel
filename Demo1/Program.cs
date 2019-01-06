using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Demo1
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
                .UseStartup<Startup>()
                .UseKestrel((context, options) =>
                {
                    //设置最大连接数,默认不受限制
                    {
                        options.Limits.MaxConcurrentConnections = 100;

                        //对于已从 HTTP 或 HTTPS 升级到另一个协议（例如，Websocket 请求）的连接,使用下面这个属性设置最大连接数.
                        options.Limits.MaxConcurrentUpgradedConnections = 100;
                    }

                    //请求正文限制,默认最大 30,000,000 字节,约 28.6 MB.
                    {
                        options.Limits.MaxRequestBodySize = 10 * 1024;
                    }

                    /*请求正文最小数据速率和宽限期
                      Kestrel 每秒检查一次数据是否以指定的速率（字节/秒）传入.如果速率低于最小值，则连接超时。
                      宽限期是 Kestrel 提供给客户端用于将其发送速率提升到最小值的时间量.在此期间不会检查速率.宽限期有助于避免最初由于 TCP 慢启动而以较慢速率发送数据的连接中断。
                      默认的最小速率为 240 字节/秒，包含 5 秒的宽限期。
                      最小速率也适用于响应。
                     *
                     */
                    {
                        options.Limits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
                        options.Limits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
                    }
                });
        }
    }
}
