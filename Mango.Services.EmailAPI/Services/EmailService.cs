using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;
        private readonly IEmailDispatcher _emailDispatcher;

        public EmailService(DbContextOptions<AppDbContext> options, IEmailDispatcher emailDispatcher)
        {
            this._dbOptions = options;
            this._emailDispatcher = emailDispatcher;
        }

        public async Task<bool> SendCartEmail(CartDto cartDto)
        {
            var message = GetFormattedMessage(cartDto);
            EmailLog emailLog = new()
            {
                EmailAddress = cartDto.CartHeader.Email,
                SentOn = DateTime.Now,
                Message = message
            };

            try
            {
                await using var _db = new AppDbContext(_dbOptions);
                _db.Emails.Add(emailLog);
                await _db.SaveChangesAsync();

                var subject = "Your Cart";
                return await _emailDispatcher.SendEmailAsync(emailLog.EmailAddress, subject, emailLog.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;                
            }            
        }

        public async Task<bool> SendOrderConfirmation(RewardDto rewardDto)
        {
            var message = GetFormattedMessage(rewardDto);
            EmailLog emailLog = new()
            {
                EmailAddress = "<To add email address to rewardDto>",
                SentOn = DateTime.Now,
                Message = message
            };

            try
            {
                await using var _db = new AppDbContext(_dbOptions);
                _db.Emails.Add(emailLog);
                await _db.SaveChangesAsync();

                var subject = "Order placed successfully!";
                return await _emailDispatcher.SendEmailAsync(emailLog.EmailAddress, subject, emailLog.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendRegistrationEmail(RegistrationDto registerDto)
        {
            var message = GetFormattedMessage(registerDto);
            EmailLog emailLog = new()
            {
                EmailAddress = registerDto.Email,
                SentOn = DateTime.Now,
                Message = message
            };

            try
            {
                await using var _db = new AppDbContext(_dbOptions);
                _db.Emails.Add(emailLog);
                await _db.SaveChangesAsync();

                var subject = "Registration successful!";
                return await _emailDispatcher.SendEmailAsync(emailLog.EmailAddress, subject, emailLog.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private string GetFormattedMessage(CartDto cartDto)
        {
            var message = new StringBuilder();
            message.AppendLine("<h1>Your Cart</h1>");
            message.AppendLine($"<h3>Total: {cartDto.CartHeader.CartTotal.ToString("C")} </h3>");
            message.AppendLine("<p><ul>");
            foreach(var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append($"{item.Product.Name} x {item.Qty}");
                message.AppendLine("</li>");
            }
            message.AppendLine("</ul></p>");
            return message.ToString();
        }

        private string GetFormattedMessage(RegistrationDto registerDto)
        {
            var message = new StringBuilder();
            message.AppendLine($"<h1>Welcome {registerDto.Name}</h1>");            
            message.AppendLine("<p>Thank you for registering</p>");
            return message.ToString();
        }

        private string GetFormattedMessage(RewardDto rewardDto)
        {
            var message = new StringBuilder();
            message.AppendLine($"<h1>Order Confirmation</h1>");
            message.AppendLine($"<p>Your order has been placed. Order ID: {rewardDto.OrderId} </p>");
            return message.ToString();
        }
    }
}
