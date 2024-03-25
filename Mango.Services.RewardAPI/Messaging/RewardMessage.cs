namespace Mango.Services.RewardAPI.Messaging
{
    public class RewardMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime RewardDate { get; set; }
        public int RewardPoint { get; set; }
        public int OrderId { get; set; }
    }
}
