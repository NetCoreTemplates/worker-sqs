using Amazon;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.Aws.Sqs;
using MyApp.ServiceModel;

namespace MyApp;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build()
            .UseServiceStack(new GenericAppHost(typeof(MyService).Assembly)
            {
                ConfigureAppHost = host =>
                {
                    var mqServer = host.Resolve<IMessageService>();
                    mqServer.RegisterHandler<Hello>(host.ExecuteMessage);
                }
            })
            .Run();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IMessageService>(new SqsMqServer(
                    hostContext.Configuration.GetConnectionString("AwsAccessKey"),
                    hostContext.Configuration.GetConnectionString("AwsSecretKey"),
                    RegionEndpoint.USEast1)
                {
                    DisablePublishingToOutq = true,
                    DisableBuffering = true, // Trade-off latency vs efficiency
                });
                services.AddHostedService<MqWorker>();
            });
}
