using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment
{
    public  class User
    {
        public int IDService { get; } = 15;
        public string Name { get; set; }

        public int UserID { get; set; }

        public string RFID { get; set; }

        public static List<string> CheckUser(string RFID)
        {
            List<string> list = new List<string>();
            FASEntities fas = new FASEntities();

            var L = fas.FAS_Users.Where(c => c.RFID == RFID).Select(c =>
            new
            {
               name = c.UserName,
               service = fas.EP_Service.Where(b => b.ID == c.IDService).FirstOrDefault().ID.ToString()


            }).ToList();

            foreach (var item in L)
            {
                list.Add(item.name);
                list.Add(item.service);
            }

            return list;
        }
    }
}