using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.Aws.Sqs;
using MyApp.ServiceModel;

namespace MyApp
{
    public class Program
    {
        public static ServiceStackHost? AppHost { get; set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    AppHost = new GenericAppHost(typeof(MyService).Assembly)
                    {
                        ConfigureAppHost = host =>
                        {
                            var mqServer = new SqsMqServer(
                                hostContext.Configuration.GetConnectionString("AwsAccessKey"),
                                hostContext.Configuration.GetConnectionString("AwsSecretKey"),
                                RegionEndpoint.USEast1)
                            {
                                DisablePublishingToOutq = true,
                                DisableBuffering = true, // Trade-off latency vs efficiency
                            };
                            mqServer.RegisterHandler<Hello>(host.ExecuteMessage);
                            host.Register<IMessageService>(mqServer);
                        }
                    }.Init();

                    services.AddSingleton(AppHost.Resolve<IMessageService>());
                    services.AddHostedService<MqWorker>();
                });
    }
}
