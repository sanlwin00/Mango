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

        private readonly IConfiguration _configuration;
        private EmailService _emailService;
        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _registrationEmailProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;
            
            _svcBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            string emailQueueName = _configuration.GetValue<string>("MessageQueueNames:OrderEmailQueue");
            string registrationQueueName = _configuration.GetValue<string>("MessageQueueNames:RegisterEmailQueue");

            var client = new ServiceBusClient(_svcBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });                     
            _emailCartProcessor = client.CreateProcessor(emailQueueName);                     
            _registrationEmailProcessor = client.CreateProcessor(registrationQueueName);
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartMessageReceived;
            _emailCartProcessor.ProcessErrorAsync += OnEmailCartMessageErrorOccured;
            await _emailCartProcessor.StartProcessingAsync();

            _registrationEmailProcessor.ProcessMessageAsync += OnRegistrationMessageReceived;
            _registrationEmailProcessor.ProcessErrorAsync += OnRegistrationMessageErrorOccured;
            await _registrationEmailProcessor.StartProcessingAsync();
        }

        private async Task OnEmailCartMessageReceived(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                Debug.WriteLine($"Processing {arg.Message.CorrelationId}");
                _emailService.SendCartEmail(cartDto);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task OnRegistrationMessageReceived(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RegistrationDto registeratioinDto = JsonConvert.DeserializeObject<RegistrationDto>(body);
            try
            {
                Debug.WriteLine($"Processing {arg.Message.CorrelationId}");
                _emailService.SendRegistrationEmail(registeratioinDto);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Task OnEmailCartMessageErrorOccured(ProcessErrorEventArgs arg)
        {
            Debug.WriteLine(arg.Exception.Message);
            return Task.CompletedTask;
        }
        private Task OnRegistrationMessageErrorOccured(ProcessErrorEventArgs arg)
        {
            Debug.WriteLine(arg.Exception.Message);
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            await _registrationEmailProcessor.StopProcessingAsync();
            await _registrationEmailProcessor.DisposeAsync();
        }
    }
}
