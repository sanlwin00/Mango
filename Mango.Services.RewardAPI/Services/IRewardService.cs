using Mango.Services.RewardAPI.Messaging;

namespace Mango.Services.RewardAPI.Services
{
    public interface IRewardService
    {
        Task<bool> UpdateRewards(RewardMessage rewardMessage);
    }
}
