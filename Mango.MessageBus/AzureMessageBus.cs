
using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Mango.MessageBus;

public class AzureMessageBus : IMessageBus
{
    private readonly string _connectionString;

    public AzureMessageBus(string connectionString)
    {
        this._connectionString = connectionString;
    }
    public async Task PublishMessageAsync(object message, string queueName)
    {
        // by default, SBClient will connect using port 9354 AMQP tcp protocol. Use AmqpWebSocket to connect using 443
        await using var client = new ServiceBusClient(_connectionString, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets});        

        ServiceBusSender sender = client.CreateSender(queueName);

        var jsonMessage = JsonConvert.SerializeObject(message);

        ServiceBusMessage svbMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage)){ 
                CorrelationId = Guid.NewGuid().ToString()
        };

        await sender.SendMessageAsync(svbMessage);
        await client.DisposeAsync();
    }
}
