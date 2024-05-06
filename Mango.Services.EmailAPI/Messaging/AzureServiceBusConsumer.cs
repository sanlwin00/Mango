using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using Serilog;
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
        private ServiceBusProcessor _orderConfrimationEmailProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;
            
            _svcBusConnectionString = _configuration.GetValue<string>("Azure:ServiceBusConnectionString");
            string emailQueueName = _configuration.GetValue<string>("MessageQueueNames:OrderEmailQueue");
            string registrationQueueName = _configuration.GetValue<string>("MessageQueueNames:RegisterEmailQueue");
            string orderCreatedTopicName = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedTopic");
            string orderCreatedSubscription = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedSubscription");

            var client = new ServiceBusClient(_svcBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });                     
            _emailCartProcessor = client.CreateProcessor(emailQueueName);                     
            _registrationEmailProcessor = client.CreateProcessor(registrationQueueName);
            _orderConfrimationEmailProcessor = client.CreateProcessor(orderCreatedTopicName, orderCreatedSubscription);
        }

        public async Task Start()
        {
            Log.Information("## Starting Azure ServiceBusProcessors...");

            _emailCartProcessor.ProcessMessageAsync += OnEmailCartMessageReceived;
            _emailCartProcessor.ProcessErrorAsync += OnEmailCartMessageErrorOccured;
            await _emailCartProcessor.StartProcessingAsync();

            _registrationEmailProcessor.ProcessMessageAsync += OnRegistrationMessageReceived;
            _registrationEmailProcessor.ProcessErrorAsync += OnRegistrationMessageErrorOccured;
            await _registrationEmailProcessor.StartProcessingAsync();

            _orderConfrimationEmailProcessor.ProcessMessageAsync += OnOrderCreatedMessageReceived;
            _orderConfrimationEmailProcessor.ProcessErrorAsync += OnOrderCreatedMessageErrorOccured;
            await _orderConfrimationEmailProcessor.StartProcessingAsync();
            
            Log.Information("## ServiceBusProcessors started.");
        }

        private async Task OnOrderCreatedMessageReceived(ProcessMessageEventArgs arg)
        {
            Log.Information("## OrderCreated Message Received.");
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardDto rewardDto = JsonConvert.DeserializeObject<RewardDto>(body);
            try
            {
                Log.Information("## Processing msg : {CorrelationId}", arg.Message.CorrelationId);
                _emailService.SendOrderConfirmation(rewardDto);
                await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task OnEmailCartMessageReceived(ProcessMessageEventArgs arg)
        {
            Log.Information("## EmailCart Message Received.");
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                Log.Information("## Processing msg : {CorrelationId}", arg.Message.CorrelationId);
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
                Log.Information("## Processing msg : {CorrelationId}", arg.Message.CorrelationId);
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
            Log.Error("Exception occured: {@ex}", arg.Exception);
            return Task.CompletedTask;
        }
        private Task OnRegistrationMessageErrorOccured(ProcessErrorEventArgs arg)
        {
            Log.Error("Exception occured: {@ex}", arg.Exception);
            return Task.CompletedTask;
        }
        private Task OnOrderCreatedMessageErrorOccured(ProcessErrorEventArgs arg)
        {
            Log.Error("Exception occured: {@ex}", arg.Exception);
            return Task.CompletedTask;
        }
        public async Task Stop()
        {
            Log.Information("## Stopping ServiceBusProcessors...");

            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            await _registrationEmailProcessor.StopProcessingAsync();
            await _registrationEmailProcessor.DisposeAsync();

            await _orderConfrimationEmailProcessor.StopProcessingAsync();
            await _orderConfrimationEmailProcessor.DisposeAsync();

            Log.Information("## ServiceBusProcessors stopped.");
        }
    }
}
