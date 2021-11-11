using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace WorkerHrEmail.Services
{
    public class EmailMessage: MailMessage
    {
        public Dictionary<string, string> _data = new Dictionary<string, string>();
        public Dictionary<string, string> Data => _data;

        public EmailMessage(string from, string to, string subject, string filename, params Tuple<string, string>[] data) : base(from, to, subject, "")
        {
            IsBodyHtml = true;
            BodyEncoding = Encoding.GetEncoding("UTF-8");

            foreach (var d in data)
            {
                Data[d.Item1] = d.Item2;
            }

            if (filename == null)
            {
                InitBody();
            }
            else
            {
                InitBodyFromTemplate(File.ReadAllText(filename));
            }
        }

        protected virtual void InitBody()
        {
            throw new Exception("not implementation yet");
        }

        /// <summary>
        /// добавить картинку в ресурсы
        /// </summary>
        /// <param name="filename">имя файла</param>
        /// <param name="contentId">идентификатор, который будет использоваться в разметке: src="cid:contentId"</param>
        protected void AddImageToResources(string filename, string contentId)
        {
            LinkedResource logo = new LinkedResource(filename, MediaTypeNames.Image.Jpeg);
            logo.ContentId = contentId;
            logo.ContentType = new System.Net.Mime.ContentType("image/jpg");
            Av.LinkedResources.Add(logo);
            _resources.Add(contentId);
        }

        private List<string> _resources = new List<string>();
        public IEnumerable<string> Resources => _resources;

        protected void AddAttachment(string filename)
        {
            Attachments.Add(new Attachment(filename));
        }

        private AlternateView Av
        {
            get
            {
                if (_av == null)
                {
                    _av = AlternateView.CreateAlternateViewFromString(Body, null, MediaTypeNames.Text.Html);
                    AlternateViews.Add(_av);
                }

                return _av;
            }
        }
        AlternateView _av = null;

        protected StringBuilder html
        {
            get
            {
                return new StringBuilder();
            }
        }

        #region парсинг шаблона
        private void InitBodyFromTemplate(string template)
        {
            var lines = template.Split('\n');
            var sb = new StringBuilder();
            var row = 0;
            var actions = new List<Action>();
            foreach (var l in lines)
            {
                row++;
                if (l.ToLower().StartsWith("@attachment")) //строка с вложением
                {
                    actions.Add(() => ParseAttachment(l.Replace("@attachment", "")));
                    continue;
                }

                if (l.ToLower().StartsWith("@cid")) //строка с картинкой
                {
                    actions.Add(() => ParseResource(l.Replace("@cid", "")));
                    continue;
                }
                sb.AppendLine(l); //просто html, сохраняем для тела
            }

            //В начале нужно проинициализироват тело, а потом уже добавлять ресурсы. Поэтому - так
            Body = sb.ToString();
            foreach (var k in Data.Keys)
                Body = Body.Replace($"{{{k}}}", Data[k]);

            actions.ForEach(x => x());
        }

        private void ParseAttachment(string filename)
        {
            //@attachment Адресная_книга_инструкция.pdf
            var dir = Directory.GetCurrentDirectory();
            if (!File.Exists(filename.Trim()))
                throw new Exception($"File {filename} not found");
            AddAttachment(filename.Trim());
        }

        private void ParseResource(string line)
        {
            //@resource topLogo = top.jpg
            var pp = line.Split('=').Select(x => x.Trim()).ToList();//отбрасываем первыйкл

            var cid = pp[0].Trim();
            var filename = pp[1].Trim();

            if (!File.Exists(filename))
                throw new Exception($"File {filename} not found");

            AddImageToResources(filename, cid);
        }

        #endregion
    }
}
