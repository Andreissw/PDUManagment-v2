using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes.Create
{
    public class MesPGName
    {
        public MesPGName()
        {
            LinePGGname = new List<LinePGGname>();
        }
        public byte? Line { get; set; }
        public string TOPBOT { get; set; }
        public List<LinePGGname> LinePGGname { get; set; }
    }

    public class LinePGGname
    { 
        public string Name { get; set; }

        public string PGName { get;set; }
    }
}