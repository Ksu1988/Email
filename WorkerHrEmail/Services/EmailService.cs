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
            //string username,
            //string password,
            //string server,
            MailMessage message)
        {
            ////var mailAuthentication = new NetworkCredential(username, password);
            //using (var mailClient = new SmtpClient(server))
            //{
            //    //mailClient.EnableSsl = true;
            //    //mailClient.UseDefaultCredentials = false;
            //    //mailClient.Credentials = mailAuthentication;
            //    mailClient.Send(message);
            //}

            var mail = new MailMessage();
            mail.To.Clear();
            mail.To.Add(new MailAddress("Aleksandr.Anufriev@stada.ru"));
            mail.Subject = "test";
            mail.From = new MailAddress("noreply@stada.ru");
            mail.IsBodyHtml = true;
            mail.Body = "testtesttest";

            var smtp = new SmtpClient("mail.stada.ru");
            //smtp.Send(mail);
        }
    }
}
