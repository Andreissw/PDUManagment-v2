using PDUManagment.Classes.Create;
using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes
{
    public class Database
    {

        CreateClass CreateOrder { get; set; }
        int UserID { get; set; }
        int LOTID { get; set; }
        int ProtocolID { get; set; }
        string protocolName { get; set; }

        FASEntities fas;


        public Database(CreateClass createOrder, int userID, int lotid)
        {
            CreateOrder = createOrder;
            UserID = userID;
            LOTID = lotid;
            fas = new FASEntities();
        }

        public void GenerateNameProtocol(int iter)
        {
            protocolName = LOTID.ToString() + "||"+ fas.FAS_GS_LOTs.Where(c => c.FULL_LOT_Code == CreateOrder.Order).Select(c => fas.FAS_Models.Where(b => b.ModelID == c.ModelID).FirstOrDefault().ModelName).FirstOrDefault()
                    + "Protocol №" + iter.ToString();
        }

        public void GeneratePGName()
        {
            var listlots = fas.FAS_GS_LOTs.Select(c => new LotsInfo
            {
                LOTID = c.LOTID,
                LotCode = c.LOTCode,
                ModelName = fas.FAS_Models.Where(b=>b.ModelID == c.ModelID).Select(b=>b.ModelName).FirstOrDefault(),
                Spec = c.Specification,
                FullLotCode = c.FULL_LOT_Code,

            }).ToList();

            listlots.AddRange(fas.Contract_LOT.Select(c=> new LotsInfo {

                LOTID = c.ID,
                LotCode =  (short)c.LOTCode,
                ModelName = fas.FAS_Models.Where(b => b.ModelID == c.ModelID).Select(b => b.ModelName).FirstOrDefault(),
                Spec = c.Specification,
                FullLotCode = c.FullLOTCode,

            }));

            var name = listlots.Where(c => c.LOTID == LOTID).Select(c => ProtocolID + "SP-" + c.LotCode + c.ModelName).FirstOrDefault().Replace(" ","_");

            var lsitbtobbot = fas.EP_Protocols.Where(c => c.ID == ProtocolID).FirstOrDefault().TOPBOT == true ? new List<string> { "TOP", "BOT" } : new List<string>() { "TOP" };


            foreach (var item in lsitbtobbot)
            {
                var ListPG = fas.EP_Machine.Select(c => new  { Name = name  + c.Name + item , IDMachine = c.ID, Type = item, IDProtocol = ProtocolID,Visible = true }).ToList();

                List<EP_PGName> eP_PGName = new List<EP_PGName>();
                foreach (var i in ListPG)
                {
                    EP_PGName ep = new EP_PGName()
                    {
                        IDMachine = i.IDMachine,
                        IDProtocol = i.IDProtocol,
                        Name = i.Name,
                        Type = i.Type,
                        Visible = i.Visible,
                    };

                    eP_PGName.Add(ep);
                }

                fas.EP_PGName.AddRange(eP_PGName); fas.SaveChanges();
            }
           
        }

     
        //public void SetLog()
        //{
        //    EP_Log log = new EP_Log()
        //    {
        //        IDProtocol = ProtocolID,
        //        UserID = (short)UserID,
        //        ServiceID = GetServiceID(),
        //        Date = DateTime.UtcNow.AddHours(2),
        //        Description = $"Клиент: {order.ClientName} " +
        //        $"|Заказ: {order.Order}  |Спецификация {order.SpecificationBom}|Сторона платы {order.TOPBOTName}",
        //        IDStep = 4,

        //    };

        //    fas.EP_Log.Add(log);
        //    fas.SaveChanges();            
        //}

        public void CreateDocument(string name, string Path)
        {
            EP_Doc doc = new EP_Doc()
            {
                IDProtocol = ProtocolID,
                Name = name,
                Path = Path,
                Visible = true,
            };

            fas.EP_Doc.Add(doc);
            fas.SaveChanges();
        }

        //public bool CreateProtocol()
        //{
        //    EP_Protocols ep = new EP_Protocols()
        //    {
        //        DateCreate = DateTime.UtcNow.AddHours(2),
        //        LOTID = LOTID,
        //        TOPBOT = GetTopBot(CreateOrder.TOPBOTName),
        //        IsActive = true,
        //        StartStatusTOP = false,
        //        StartStatusBOT = false,
        //        NameProtocol = protocolName,
        //        //ProgrammName = CreateOrder.ProgrammName,
        //    };

        //    fas.EP_Protocols.Add(ep);
        //    fas.SaveChanges();
        //    ProtocolID = fas.EP_Protocols.OrderByDescending(c => c.DateCreate).FirstOrDefault().ID;
        //    return (bool)ep.TOPBOT;           
        //}      


        public void AddInfo(bool topbot)
        {          
                            
            var IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c=>c.Manufacter != "Цех поверхностного монтажа").ToList();
            addinfo(IdList);

            IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c => c.Manufacter == "Цех поверхностного монтажа").ToList();

            foreach (var item in new List<string>() {"TOP","BOT" })           
                addinfo(IdList, item);           


            void addinfo(List<EP_TypeVerification> list, string TOP = "TOP")
            {
                foreach (var item in list)
                {
                    EP_ProtocolsInfo INFO = new EP_ProtocolsInfo()
                    {
                        TypeVerifID = item.ID,
                        DateCreate = DateTime.UtcNow.AddHours(2),
                        ProtocolID = ProtocolID,
                        Signature = false,
                        TOPBOT = TOP,
                        Visible = true,
                    };

                    fas.EP_ProtocolsInfo.Add(INFO);
                }

                fas.SaveChanges();
            }
        }

        bool GetTopBot(string name)
        {
            return name == "TOP" ? false : true;
        }

       

    }
}