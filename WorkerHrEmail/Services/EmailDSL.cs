using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerHrEmail.Services
{
    /// <summary>
    /// простой генератор html шаблонов
    /// </summary>
    public static class EmailDSL
    {
        public static StringBuilder Img(this StringBuilder sb, string ContentId, string attrs = null)
        {
            if (attrs != null)
                sb.Append($"<img src=\"cid:{ContentId}\" {attrs}/>");
            else
                sb.Append($"<img src=\"cid:{ContentId}\" />");
            return sb;
        }

        public static StringBuilder Br(this StringBuilder sb)
        {
            sb.AppendLine("<br/>");
            return sb;
        }

        public static StringBuilder Text(this StringBuilder sb, string content)
        {
            sb.Append(content);
            return sb;
        }

        public static StringBuilder Textln(this StringBuilder sb, string content)
        {
            sb.AppendLine(content);
            return sb;
        }


        /// <summary>
        /// ссылка
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="href"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static StringBuilder HyperLink(this StringBuilder sb, string href, string text)
        {
            sb.Append($"<a href=\"{href}\">{text}</a>");
            return sb;
        }

        public static StringBuilder HyperLink(this StringBuilder sb, string href, StringBuilder content)
        {
            sb.Append($"<a href=\"{href}\">{content}</a>");
            return sb;
        }

        /// <summary>
        /// Параграф
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static StringBuilder P(this StringBuilder sb, string text, string attrs = null)
        {
            if (attrs == null)
                sb.AppendLine($"<p>{text}</p>");
            else
                sb.AppendLine($"<p {attrs}>{text}</p>");
            return sb;
        }

        public static StringBuilder P(this StringBuilder sb, StringBuilder content, string attrs = null)
        {
            sb.P(content.ToString(), attrs);
            return sb;
        }

        #region списки
        public static StringBuilder Ul(this StringBuilder sb, string content, string attrs = null)
        {
            if (attrs != null)
                sb.AppendLine($"<ul {attrs}>");
            else
                sb.AppendLine("<ul>");

            sb.Append(content);
            sb.AppendLine("</ul>");

            return sb;
        }

        public static StringBuilder Ul(this StringBuilder sb, StringBuilder content, string attrs = null)
        {
            return sb.Ul(content.ToString(), attrs);
        }


        public static StringBuilder Li(this StringBuilder sb, string text)
        {
            sb.AppendLine($"<li>{text}</li>");
            return sb;
        }

        public static StringBuilder Li(this StringBuilder sb, StringBuilder content)
        {
            return sb.Li(content.ToString());
        }
        #endregion списки

        #region таблицы
        public static StringBuilder Table(this StringBuilder sb, string content, string attrs = null)
        {
            if (attrs != null)
                sb.AppendLine($"<table {attrs}>");
            else
                sb.AppendLine($"<table>");
            sb.AppendLine(content);
            sb.AppendLine("</table>");

            return sb;
        }

        public static StringBuilder Table(this StringBuilder sb, StringBuilder content, string attrs = null)
        {
            return sb.Table(content.ToString(), attrs);
        }


        public static StringBuilder Tr(this StringBuilder sb, string content, string attrs = null)
        {
            if (attrs != null)
                return sb.AppendLine($"<tr {attrs}>{content}></tr>");
            else
                return sb.AppendLine($"<tr>{content}></tr>");
        }
        public static StringBuilder Tr(this StringBuilder sb, StringBuilder content, string attrs = null)
        {
            return sb.Tr(content.ToString(), attrs);
        }

        public static StringBuilder Td(this StringBuilder sb, string content, string attrs = null)
        {
            if (attrs != null)
                return sb.AppendLine($"<td {attrs}>{content}></td>");
            else
                return sb.AppendLine($"<td>{content}></td>");
        }
        public static StringBuilder Td(this StringBuilder sb, StringBuilder content, string attrs = null)
        {
            return sb.Td(content.ToString(), attrs);
        }

        #endregion таблицы
    }
}
