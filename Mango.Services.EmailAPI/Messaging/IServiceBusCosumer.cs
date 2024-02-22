namespace Mango.Services.EmailAPI.Messaging
{
    public interface IServiceBusCosumer
    {
        Task Start();
        Task Stop();
    }
}
