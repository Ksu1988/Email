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
            message.To.Clear();
            message.To.Add(new MailAddress("aleksandr.anufriev@stada.ru"));

            //var mailAuthentication = new NetworkCredential(username, password);
            using (var mailClient = new SmtpClient("mail.stada.ru"))
            {
                //mailClient.Send(message);
            }

            //var mail = new MailMessage();
            //mail.To.Clear();
            //mail.To.Add(new MailAddress("Aleksandr.Anufriev@stada.ru"));
            //mail.Subject = "test";
            //mail.From = new MailAddress("noreply@stada.ru");
            //mail.IsBodyHtml = true;
            //mail.Body = "testtesttest";

            //var smtp = new SmtpClient("mail.stada.ru", 25);
            //smtp.Send(mail);
        }
    }
}
