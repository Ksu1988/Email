using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerHrEmail.Services
{
    /// <summary>
    /// Письмо-отчет о том кому и что послали за последний месяц
    /// </summary>
    public class EmailReport: EmailMessage
    {
        public EmailReport(string to, string subject, string filename)
            : base("noreply@stada.ru", to, "Отчет о письмах сотрудникам")
        {

        }

        protected override void InitBody()
        {
            Body = html
                .Text("<H3>Отчет ")



                .ToString();
        }
    }
}
