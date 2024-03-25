namespace Mango.Services.RewardAPI.Messaging
{
    public interface IServiceBusCosumer
    {
        Task Start();
        Task Stop();
    }
}
