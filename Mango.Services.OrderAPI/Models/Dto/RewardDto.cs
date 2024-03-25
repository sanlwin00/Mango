namespace Mango.Services.OrderAPI.Models
{
    public class RewardDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime RewardDate { get; set; }
        public int RewardPoint { get; set; }
        public int OrderId { get; set; }
    }
}
