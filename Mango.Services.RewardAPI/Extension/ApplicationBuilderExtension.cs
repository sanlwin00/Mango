using Mango.Services.RewardAPI.Messaging;
using System.Runtime.CompilerServices;

namespace Mango.Services.RewardAPI.Extension
{
    public static class ApplicationBuilderExtension
    {
        private static IServiceBusCosumer _serviceBusCosumer {  get; set; }

        public static IApplicationBuilder UseServiceBusConsumer(this IApplicationBuilder app)
        {
            _serviceBusCosumer = app.ApplicationServices.GetService<IServiceBusCosumer>();
            
            var hostApplicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            hostApplicationLifetime.ApplicationStarted.Register(OnStart);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);

            return app;
        }

        private static void OnStopping()
        {
            _serviceBusCosumer.Stop();
        }

        private static void OnStart()
        {
            _serviceBusCosumer.Start();
        }
    }
}
