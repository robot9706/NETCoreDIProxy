using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace DITest
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                 .ConfigureServices((services) =>
                 {
                     // Add the implementation which does not actually implement the service interface
                     services.AddSingleton<ExampleServiceImplementation>();

                     // Create a proxy which implements IExampleServiceInterface and invokes methods in ExampleServiceImplementation
                     services.AddProxy<IExampleServiceInterface, ExampleServiceImplementation>();

                     // Add a test service
                     services.AddHostedService<TestService>();
                 })
            .Build().Run();
        }
    }

    class Proxy<TDecorated> : DispatchProxy
    {
        private object implementation;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
             MethodInfo mirrorMethod = implementation.GetType().GetMethods().Where(m => m.ToString() == targetMethod.ToString()).FirstOrDefault();

            return mirrorMethod.Invoke(implementation, args);
        }

        private void SetImplementation(object impl)
        {
            implementation = impl;
        }

        public static TDecorated Create(object impl)
        {
            object proxy = Create<TDecorated, Proxy<TDecorated>>();
            ((Proxy<TDecorated>)proxy).SetImplementation(impl);

            return (TDecorated)proxy;
        }
    }

    static class Extensions
    {
        public static void AddProxy<TService, TImplementation>(this IServiceCollection services)
            where TService : class
        {
            services.AddSingleton<TService>((provider) =>
            {
                var implementation = provider.GetRequiredService<TImplementation>();

                return Proxy<TService>.Create(implementation);
            });
        }
    }

    interface IExampleServiceInterface
    {
        string Test();
    }

    // Implemented somewhere else where "IExampleServiceInterface" is not accessible
    class ExampleServiceImplementation // Does not implement IExampleServiceInterface
    {
        public string Test()
        {
            return "Hello through proxy!";
        }
    }

    class TestService : IHostedService
    {
        public TestService(IExampleServiceInterface test, ILogger<TestService> logger)
        {
            string text = test.Test();

            logger.LogInformation($"Service through proxy says: '{text}'");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
