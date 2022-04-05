using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace WorkerHrEmail.Services
{
    public class EmailService
    {
        public static void SendMessage(
            MailMessage message)
        {
            //TODO !!! две следующих строки - для тестирования !!!
//            message.To.Clear();
            message.To.Add(new MailAddress("aleksandr.anufriev@stada.ru"));
//            message.CC.Add(new MailAddress("alexey.grigin@stada.ru"));

            using (var mailClient = new SmtpClient("mail.stada.ru"))
            {
                mailClient.Send(message);
            }
        }
    }
}
