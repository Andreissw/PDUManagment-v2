using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace PDUManagment.Classes
{
    public class Email
    {
        public static void RunEmailFAS(string subject, string Content)
        {
            var view = AlternateView.CreateAlternateViewFromString(Content, Encoding.UTF8, MediaTypeNames.Text.Html);

            using (var client = new SmtpClient("mail.technopolis.gs", 25)) // Set properties as needed or use config file
            using (MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,

            })

            {

                message.AlternateViews.Add(view);
                message.From = new MailAddress("reporter@dtvs.ru", "ROBOT");
                message.CC.Add("a.volodin@dtvs.ru");
                message.CC.Add("Лишик Станислав Александрович <lishik@dtvs.ru>");
                message.CC.Add("Мелехин Константин Данилович <melekhin@dtvs.ru>");
                message.CC.Add("Овчинников Дмитрий Игоревич <ovchinnikov@dtvs.ru>");
                message.CC.Add("Шишкин Игорь Алексеевич <i.shishkin@dtvs.ru>");
                client.Send(message);

            }


        }
    }
}