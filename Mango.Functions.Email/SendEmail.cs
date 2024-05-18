using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Mango.Functions.Email
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string toEmail = data?.email;
            string subject = data?.subject;
            string message = data?.message;

            try
            {
                // Build configuration
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // Read SMTP settings from configuration
                var smtpHost = config["SmtpHost"];
                var smtpPort = int.Parse(config["SmtpPort"]);
                var smtpUsername = config["SmtpUsername"];
                var smtpPassword = config["SmtpPassword"];
                var smtpEnableSsl = bool.Parse(config["SmtpEnableSsl"]);
                var smtpFrom = config["SmtpFrom"];

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = smtpEnableSsl,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFrom),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                smtpClient.Send(mailMessage);
                return new OkObjectResult("Email sent successfully");
            }
            catch (Exception ex)
            {
                log.LogError($"Error sending email: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
