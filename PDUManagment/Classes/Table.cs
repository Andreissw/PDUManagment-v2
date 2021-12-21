using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes
{
    public class Table
    {
        public int ID { get; set; }

        public string TypeClient { get; set; }

        public bool IsActive { get; set; }

        public string ProtocolName { get; set; }

        public string NameClient { get; set; }

        public string Name { get; set; }

        public string Spec { get; set; }

        public List<ProgrammNames> ProgrammName { get; set; }

        public string TOPBOT { get; set; }

        public DateTime DateCreate { get; set; }

        public DateTime DateEdit { get; set; }
    }

    public class ProgrammNames
    {
        public string ProtocolName { get; set; }
        public string Name { get; set; }
        public string TOPBOT { get; set; }
        public string Machine { get; set; }
        public string PGName { get; set; }
    }
}