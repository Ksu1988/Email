using System;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace WorkerHrEmail.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;
        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        public  void SendMessage(MailMessage message)
        {
            try
            {
                if (message.From == null)
                {
                    var from = _config.GetSection("Email:From").Value;
                    message.From = new MailAddress(from);
                }
                var server = _config.GetSection("Email:Server").Value;
                var srtPort = _config.GetSection("Email:Port").Value;
                var defaultport = 25;
                var port = 0;
                int.TryParse(srtPort, out port);
                var login = _config.GetSection("Email:Login").Value;
                var password = _config.GetSection("Email:Password").Value;
                _logger.LogDebug($"{server}, {login}, {password}, {message.From}");
                using var mailClient = new SmtpClient(server, port == 0 ? defaultport : port);
                // mailClient.Credentials = new System.Net.NetworkCredential(login, password);

                mailClient.Send(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            
        }
    }
}
