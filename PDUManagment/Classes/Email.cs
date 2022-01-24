using PDUManagment.Models;
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
            FASEntities fas = new FASEntities();
            var listEmail = fas.EP_Email.Where(b => b.Type == "ПДУ").Select(c => c.Email).ToList();

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

                foreach (var item in listEmail)              
                    message.CC.Add(item);

                #region
                //message.CC.Add("a.volodin@dtvs.ru");
                //message.CC.Add("Щербакова Ирина Владимировна <skyratova@dtvs.ru>");
                //message.CC.Add("Оператор баз данных THT <bdtht@dtvs.ru>");
                //message.CC.Add("Павловская Ольга Степановна <pavlovskaya@dtvs.ru>");
                //message.CC.Add("Склад SMT <skladsmt@dtvs.ru>");

                //message.CC.Add("Кладовщик ЦС <klad_cs@dtvs.ru>");
                //message.CC.Add("Овчинников Дмитрий Игоревич <ovchinnikov@dtvs.ru>");
                //message.CC.Add("Сидоров Владислав Леонидович <sidorov@dtvs.ru>");
                //message.CC.Add("Климчук Андрей Михайлович <klimchuk@dtvs.ru>");
                //message.CC.Add("Ломакина Светлана Ивановна <s.lomakina@dtvs.ru>");

                //message.CC.Add("Савельева Елена Викторовна <e.savelieva@dtvs.ru>");
                //message.CC.Add("Рыжков Иван Васильевич <i.ryjkov@dtvs.ru>");
                //message.CC.Add("Ященко Петр Владимирович <yashenko@dtvs.ru>");
                //message.CC.Add("Склад оснастки <skladosn@dtvs.ru>");
                //message.CC.Add("Мастер FAS <masterfas@dtvs.ru>");

                //message.CC.Add("Мастер SMT <mastersmt@dtvs.ru>");
                //message.CC.Add("Фролов Дмитрий Андреевич <d.frolov@dtvs.ru>");
                //message.CC.Add("Силин Илья Дмитриевич <i.silin@dtvs.ru>");
                //message.CC.Add("Коваленко Игорь Владимирович <kovalenko@dtvs.ru>");
                //message.CC.Add("Зубец Виталий Анатольевич <zubetc@dtvs.ru>");
                //message.CC.Add("Фоут Виктор Францевич <v.fout@dtvs.ru>");

                //message.CC.Add("Кобзарь Татьяна Александровна <t.kobzar@prancor.ru>");
                //message.CC.Add("Карнаухов Михаил Андреевич <m.karnauhov@dtvs.ru>");
                //message.CC.Add("Лишик Станислав Александрович <lishik@dtvs.ru>");
                //message.CC.Add("Мелехин Константин Данилович <melekhin@dtvs.ru>");
                //message.CC.Add("Каспирович Дмитрий Иванович <kaspirovich@dtvs.ru>");
                //message.CC.Add("Бондарай Александр Александрович <a.bondaray@dtvs.ru>");
                #endregion

                client.Send(message);

            }


        }
    }
}