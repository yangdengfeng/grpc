using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcService.Web.Protos;
using GrpcService.web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace GrpcService.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //作为Grpc服务端，供客服端调用 证书验证
            //const int port = 5001;
            //var cert = File.ReadAllText(@"cert.pem");//证书公钥
            //var privateKey = File.ReadAllText(@"key.pem");//证书私钥
            //var sslCredentials = new SslServerCredentials
            //(
            //    new List<KeyCertificatePair>
            //    {
            //        new KeyCertificatePair(cert, privateKey)
            //    }
            //);

            //var server = new Server
            //{
            //    Ports = { new ServerPort("localhost", port, sslCredentials)},
            //    Services = { EmployeeService.BindService(new MyEmployeeService()) }
            //};

            //server.Start();

            //Console.WriteLine($"server start on port: {port}");
            //Console.WriteLine("press any key to stop");
            //Console.ReadKey();

            //await server.ShutdownTask();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseSerilog((context, configuration) =>
                        {
                            configuration.MinimumLevel.Debug()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                .WriteTo.Console()
                                .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
                                .CreateLogger();
                        });
                });
    }
}
