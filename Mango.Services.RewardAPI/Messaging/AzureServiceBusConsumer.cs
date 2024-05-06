using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging
{
    public class AzureServiceBusConsumer : IServiceBusCosumer
    {
        private readonly string _svcBusConnectionString;

        private readonly IConfiguration _configuration;
        private RewardService _rewardService;
        private ServiceBusProcessor _rewardMessageProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _rewardService = rewardService;
            _configuration = configuration;
            
            _svcBusConnectionString = _configuration.GetValue<string>("Azure:ServiceBusConnectionString");
            string orderCreatedTopicName = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedTopic");
            string orderCreatedSubscription = _configuration.GetValue<string>("MessageQueueNames:OrderCreatedSubscription");

            var client = new ServiceBusClient(_svcBusConnectionString, new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets });                     
            _rewardMessageProcessor = client.CreateProcessor(orderCreatedTopicName, orderCreatedSubscription);                     
        }

        public async Task Start()
        {
            _rewardMessageProcessor.ProcessMessageAsync += OnRewardMessageReceived;
            _rewardMessageProcessor.ProcessErrorAsync += OnEmailCartMessageErrorOccured;
            await _rewardMessageProcessor.StartProcessingAsync();
        }

        private async Task OnRewardMessageReceived(ProcessMessageEventArgs arg)
        {
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardMessage rewardMessage = JsonConvert.DeserializeObject<RewardMessage>(body);
            try
            {
                Debug.WriteLine($"Processing {arg.Message.CorrelationId}");
                _rewardService.UpdateRewards(rewardMessage);
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

        public async Task Stop()
        {
            await _rewardMessageProcessor.StopProcessingAsync();
            await _rewardMessageProcessor.DisposeAsync();
        }
    }
}
