using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IServiceBusCosumer
    {
        private readonly string _svcBusConnectionString;
        private readonly string _emailQueueName;
        private readonly IConfiguration _configuration;
        private EmailService _emailService;
        private ServiceBusProcessor _emailCartProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;
            _svcBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _emailQueueName = _configuration.GetValue<string>("MessageQueueNames:EmailQueue");

            var client = new ServiceBusClient(_svcBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });
            _emailCartProcessor = client.CreateProcessor(_emailQueueName);
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartMessageReceived;
            _emailCartProcessor.ProcessErrorAsync += OnEmailCartMessageErrorOccured;
            await _emailCartProcessor.StartProcessingAsync();
        }

        private Task OnEmailCartMessageErrorOccured(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task OnEmailCartMessageReceived(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                Debug.WriteLine($"Processing {arg.Message.CorrelationId}");
                _emailService.SendEmailAndLog(cartDto);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();
        }
    }
}
