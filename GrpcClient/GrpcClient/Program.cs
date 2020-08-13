using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService.Web.Protos;
using Serilog;
using Serilog.Events;

namespace GrpcClient
{
    class Program
    {
        private static string _token;
        private static DateTime _expriation = DateTime.MinValue;

        static async Task Main(string[] args)
        {
            string serilogOutputTemplate = "{NewLine}{NewLine}Date：{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}LogLevel：{Level}{NewLine}Message：{Message}{NewLine}{Exception}" + new string('-', 100);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", 
                    LogEventLevel.Debug, 
                    rollingInterval: RollingInterval.Day, 
                    outputTemplate: serilogOutputTemplate)
                .CreateLogger();

            //通过证书验证方式请求服务端，可以是Go服务器或C#服务端
            //const int port = 5001;
            //var pem = File.ReadAllText(@"cert.pem"); //证书公钥
            //var sslCredentials = new SslCredentials(pem);
            //var channel = new Channel("locahost", port, SslCredentials);

            Log.Information("client starting...");
            using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                LoggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory()
            });

            var client = new EmployeeService.EmployeeServiceClient(channel);

            var option = 1; //int.Parse(args[0]);

            switch (option)
            {
                case 1:
                    await GetByNoAsync(client);
                    break;
                case 2:
                    await GetAllAsync(client);
                    break;
                case 3:
                    await AddPhotoAsync(client);
                    break;
                case 4:
                    await SaveAllAsync(client);
                    break;
                case 5:
                    await SaveAsync(client);
                    break;
            }

            Console.WriteLine("enter any key to exit...");
            Console.ReadKey();

            Log.CloseAndFlush();
        }


        private static bool NeedToken() => string.IsNullOrEmpty(_token) || _expriation < DateTime.UtcNow;



        public static async Task SaveAsync(EmployeeService.EmployeeServiceClient client)
        {
            Employee employee = new Employee
            {
                No = 888,
                FirstName = "KKK",
                LastName = "V"
            };

            var call = await client.SaveAsync(new EmployeeRequest
            {
                Employee = employee
            });

            Console.WriteLine($"response: {call}");
        }

        public static async Task SaveAllAsync(EmployeeService.EmployeeServiceClient client)
        {
            var employees = new List<Employee>
            {
                new Employee
                {
                    No = 666,
                    FirstName = "lll",
                    LastName = "H"
                },
                new Employee
                {
                    No = 777,
                    FirstName = "qqq",
                    LastName = "G"
                }
            };

            using var call = client.SaveAll();
            var requestStream = call.RequestStream;
            var responseStream = call.ResponseStream;

            var responseTask = Task.Run(async () =>
            {
                while (await responseStream.MoveNext())
                {
                    Log.Debug($"saved: {responseStream.Current.Employee}");
                    Console.WriteLine($"saved: {responseStream.Current.Employee}");
                }
            });

            foreach (var employee in employees) 
            {
                await requestStream.WriteAsync(new EmployeeRequest
                {
                    Employee = employee
                });
            }

            await requestStream.CompleteAsync();
            await responseTask;
        }

        public static async Task AddPhotoAsync(EmployeeService.EmployeeServiceClient client)
        {
            var md = new Metadata
            {
                {"username", "admin"},
                {"role", "administrator"}
            };

            FileStream fs = File.OpenRead("asd.jpg");

            using var call = client.AddPhoto(md);

            var stream = call.RequestStream;

            while (true)
            {
                byte[] buffer = new byte[1024];

                int numRead = await fs.ReadAsync(buffer, 0, buffer.Length);

                if (numRead == 0)
                {
                    break;
                }

                if (numRead < buffer.Length)
                {
                    Array.Resize(ref buffer, numRead);
                }

                await stream.WriteAsync(new AddPhotoRequest
                {
                    Data = ByteString.CopyFrom(buffer)
                });
            }

            await stream.CompleteAsync();

            var response = await call.ResponseAsync;

            Console.WriteLine(response.IsOk);
         }

        public static async Task GetAllAsync(EmployeeService.EmployeeServiceClient client)
        {
            using var call = client.GetAll(new GetAllRequest());

            var responseSteam = call.ResponseStream;
            while (await responseSteam.MoveNext())
            {
                Log.Debug($"response message : {responseSteam.Current.Employee}");
                Console.WriteLine($"response message : {responseSteam.Current.Employee}");
            }
        }

        public static async Task GetByNoAsync(EmployeeService.EmployeeServiceClient client)
        {
            try
            {
                if (!NeedToken() || await GetTokenAsync(client))
                {
                    var header = new Metadata
                    {
                        {"Authorization", $"Bearer {_token}"},
                    };

                    var response = await client.GetByNoAsync(new GetByNoRequest
                    {
                        No = 111
                    }, header);

                    Log.Logger.Debug($"response message : {response}");
                    Console.WriteLine($"response message : {response}");
                }
            }
            catch (RpcException re)
            {
                if (re.StatusCode == StatusCode.NotFound)
                {
                    Log.Logger.Error($"trailer: {re.Trailers}");
                }
                Log.Logger.Error(re.Message);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }
        }

        private static async Task<bool> GetTokenAsync(EmployeeService.EmployeeServiceClient client)
        {
            var model = new TokenRequest
            {
                Username = "admin",
                Password = "123"
            };

            var response = await client.CreateTokenAsync(model);

            if (response.Success)
            {
                _token = response.Token;
                _expriation = response.Expiration.ToDateTime();
                return true;
            }

            return false;
        }
    }
}
