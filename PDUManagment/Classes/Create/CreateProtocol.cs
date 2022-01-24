using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PDUManagment.Classes.Create
{
    public class CreateProtocol : CreateClass
    {
        public List<SelectListItem> ListOrders { get; set; }
        public CreateProtocol()
        {
            ListOrders = new List<SelectListItem>() { new SelectListItem() { Text = ""} };
        }

        public void GetCountProtocols()
        {
            using (var fas = new FASEntities())
            {
                CountProtocols =  fas.EP_Protocols.Where(c => c.LOTID == LOTID).Count();
            }
        }
       
        public override void SetLOG()
        {
            using (FASEntities fas = new FASEntities())
            {

                EP_Log log = new EP_Log()
                {
                    //IDProtocol = ProtocolID,
                    UserID = (short)UserID,
                    ServiceID = GetServiceID(),
                    Date = DateTime.UtcNow.AddHours(2),                    
                    IDStep = 6,
                };

                fas.EP_Log.Add(log);
                fas.SaveChanges();
            }
        }

    }
}
