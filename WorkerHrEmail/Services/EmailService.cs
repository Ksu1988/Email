using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace WorkerHrEmail.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public  void SendMessage(
            MailMessage message)
        {
            //TODO !!! две следующих строки - для тестирования !!!
            //message.To.Clear();
            // message.Bcc.Add(new MailAddress("kseniia.chukhareva@stada.ru"));
            if (message.From == null)
            {
                var from =  _config.GetSection("Email:From").Value;
                message.From = new MailAddress(from);
            }
            var server = _config.GetSection("Email:Server").Value; 
            var srtPort = _config.GetSection("Email:Port").Value;
            int defaultport = 25;
            int port = 0;
            int.TryParse(srtPort,out  port);
            var login = _config.GetSection("Email:Login").Value;
            var password = _config.GetSection("Email:Passwort").Value;
#if DEBUG
            server = "smtp.mailspons.com";
            login = "59dabbc7fa914d5ba1dc";
            password = "301eec189439417da24114956a201a9e";
#endif
            using (var mailClient = new SmtpClient(server,port == 0 ? defaultport : port))
            {
                mailClient.Credentials = new System.Net.NetworkCredential(login, password);
                mailClient.Send(message);
            }
        }
    }
}
