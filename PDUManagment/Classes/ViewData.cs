using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes
{
    public class ViewData
    {
        public string ClientType { get; set; }
        public string NameClient { get; set; }
        public string NameOrder { get; set; }
        public string NameSpec { get; set; }       
        public int Count { get; set; }

        public bool IsActive { get; set; }
        public DateTime DateCreate { get; set; }

        public DateTime DateManufacter { get; set; }
    }
}