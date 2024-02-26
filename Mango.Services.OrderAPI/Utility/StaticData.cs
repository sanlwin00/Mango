namespace Mango.Services.OrderAPI.Utility
{
    public class StaticData
    {
        public enum Roles
        {
            ADMIN,
            CUSTOMER
        }

        public enum OrderStatus
        {
            Pending,
            Approved,
            ReadyForPickup,
            Completed,
            Refunded,
            Cancelled
        }
    }
}
