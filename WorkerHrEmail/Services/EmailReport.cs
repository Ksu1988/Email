using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using WorkerHrEmail.Model;

namespace WorkerHrEmail.Services
{
    /// <summary>
    /// Письмо-отчет о том кому и что послали за последний месяц
    /// </summary>
    public class EmailReport: EmailMessage
    {
        public List<object> Items { set; get; }
        public EmailReport(string to, params History[] data)
            : base("noreply@stada.ru", to, "Отчет о письмах сотрудникам")
        {

        }

        protected override void InitBody()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<h3>Отчет</h3><br/>");
            sb.Append("<table><tr><td>№ п/п</td><td>Дата</td><td>ФИО</td><td>Письмо</td></tr>");

            foreach(var r in Items)
            {

            }

            sb.AppendLine("</table>");
        }
    }
}
