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

        public EmailService(DbContextOptions<AppDbContext> options)
        {
            this._dbOptions = options;
        }

        public async Task<bool> SendEmailAndLog(CartDto cartDto)
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

                return true;
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
            message.AppendLine("<br/>Your Cart");
            message.AppendLine($"<br/>Total: {cartDto.CartHeader.CartTotal}");
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
    }
}
