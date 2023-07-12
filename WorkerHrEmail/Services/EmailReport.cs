using System.Collections.Generic;
using System.Text;
using WorkerHrEmail.Model;

namespace WorkerHrEmail.Services
{
    /// <summary>
    /// Письмо-отчет о том кому и что послали за последний месяц
    /// </summary>
    public class EmailReport : EmailMessage
    {
        public List<History> Items { set; get; } = new List<History>();
        public EmailReport(string to, string from, params History[] data)
            : base(to, "Отчет о письмах сотрудникам", null, from)
        {
            Items.AddRange(data);
        }

        protected override void InitBody()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<h3>Отчет</h3><br/>");
            sb.Append("<table><tr><td>№ п/п</td><td>ФИО</td><td>Дата</td><td>Письмо</td></tr>");

            var pp = 1;
            foreach (var r in Items)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{pp}</td>");
                sb.Append($"<td>{r.LastNameRu} {r.FirstNameRu} {r.MiddleNameRu}</td>");

                if (r.WellcomeEmail != null & r.ReportWellcome == null)
                {
                    sb.Append($"<td>{r.WellcomeEmail}</td>");
                    sb.Append($"<td>wellcome</td>");
                }

                if (r.OneYearEmail != null & r.ReportOneYear == null)
                {
                    sb.Append($"<td>{r.OneYearEmail}</td>");
                    sb.Append($"<td>wellcome</td>");
                }

                sb.Append("</tr>");
                pp++;
            }

            sb.AppendLine("</table>");
        }
    }
}
