using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public RewardService(DbContextOptions<AppDbContext> options)
        {
            this._dbOptions = options;
        }

        public async Task<bool> UpdateRewards(RewardMessage rewardMessage)
        {
            Reward reward = new()
            {
                OrderId = rewardMessage.OrderId,
                RewardPoint = rewardMessage.RewardPoint,
                RewardDate = rewardMessage.RewardDate,
                UserId = rewardMessage.UserId
            };

            try
            {
                await using var _db = new AppDbContext(_dbOptions);
                _db.Rewards.Add(reward);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
