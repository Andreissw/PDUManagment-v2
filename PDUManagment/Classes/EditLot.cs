using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Classes
{
    public class EditLot
    {
        public int ProtocolID { get; set; }
        public string Order { get; set; }
        public string SpecBOM { get; set; }
        //public string SpecPath { get; set; }

        //public string ContentType { get; set; }

        //public string NameFile { get; set; }

        public string ClientName { get; set; }
        public int LOTID { get; set; }   
        public HttpPostedFileBase FileSpec { get; set; }
        public string ProgrammName { get; set; }
        public List<SelectListItem> TOPBOT { get; set; }
        public string TOPBOTName { get; set; }

    

        public string ClientType { get; set; }


    }

 
}