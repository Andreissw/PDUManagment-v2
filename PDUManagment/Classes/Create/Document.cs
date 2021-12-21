using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes.Create
{
    public class Document
    {        
       public int ID { get; set; }
       public string Name { get; set; }

       public string Path { get; set; }

       public string ContentType { get; set; }

       public string NameFile { get; set; }

        public string Extension { get; set; }


    }
}