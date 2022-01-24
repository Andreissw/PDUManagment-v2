using PDUManagment.Classes;
using PDUManagment.Classes.Create;
using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment
{
    public abstract class CreateClass
    {
        public int UserID { get; set; }
        public int LOTID { get; set; }
        //public int ProtocolID { get; set; }

        public List<int> ListProtocolID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата выпуска продукции")]
        public DateTime Date { get; set; }

       
        public List<SelectListItem> ListModel { get; set; }

        [Required]
        public string Order { get; set; }
        public string ProtocolName { get; set; }
        public bool IsTOPBOT { get; set; }
        public List<SelectListItem> TOPBOT { get; set; }
        [Required]
        public string TOPBOTName { get; set; }

        public int CountProtocols { get; set; } = 1;

        public List<Document> ListDocs { get; set; }

        public abstract void SetLOG();
        public int GetServiceID()
        {
           using (var fas = new FASEntities())
           {
               return (int)fas.FAS_Users.Where(c => c.UserID == UserID).FirstOrDefault().IDService;
           }
           
        }

        public void GeneratePGName(int protocolID)
        {
            using (var fas = new FASEntities())
            {
               
                var listlots = fas.FAS_GS_LOTs.Select(c => new LotsInfo
                {
                    LOTID = c.LOTID,
                    LotCode = c.LOTCode,
                    ModelName = fas.FAS_Models.Where(b => b.ModelID == c.ModelID).Select(b => b.ModelName).FirstOrDefault(),
                    Spec = c.Specification,
                    FullLotCode = c.FULL_LOT_Code,

                }).ToList();

                listlots.AddRange(fas.Contract_LOT.Select(c => new LotsInfo
                {

                    LOTID = c.ID,
                    LotCode = (short)c.LOTCode,
                    ModelName = fas.FAS_Models.Where(b => b.ModelID == c.ModelID).Select(b => b.ModelName).FirstOrDefault(),
                    Spec = c.Specification,
                    FullLotCode = c.FullLOTCode,

                }));

                var line = fas.EP_Protocols.Where(c => c.ID == protocolID).Select(c => c.Line).FirstOrDefault();

                var name = listlots.Where(c => c.LOTID == LOTID).Select(c => line + "_" +protocolID + "_SP_" + c.LotCode + "_" + c.ModelName).FirstOrDefault().Replace(" ", "_");

                var lsitbtobbot =  new List<string> { "TOP", "BOT" };

                foreach (var item in lsitbtobbot)
                {
                    bool visible = true;
                    if (item == " BOT")
                        visible = GetTopBot(TOPBOTName);

                    var ListPG = fas.EP_Machine.Select(c => new { Name = name +"_"+ item, IDMachine = c.ID, Type = item, IDProtocol = protocolID }).ToList();

                    List<EP_PGName> eP_PGName = new List<EP_PGName>();
                    foreach (var i in ListPG)
                    {
                        EP_PGName ep = new EP_PGName()
                        {
                            IDMachine = i.IDMachine,
                            IDProtocol = i.IDProtocol,
                            Name = i.Name,
                            Type = i.Type,
                            Visible = visible,
                        };

                        eP_PGName.Add(ep);
                    }

                    fas.EP_PGName.AddRange(eP_PGName); fas.SaveChanges();
                }
               
            }
        }

        public void CreateProtocol( byte line)
        {
            
            using (var fas = new FASEntities())
            {
                var isTOPBOT = GetTopBot(TOPBOTName);
                EP_Protocols ep = new EP_Protocols()
                {
                    DateCreate = DateTime.UtcNow.AddHours(2),
                    LOTID = LOTID,
                    TOPBOT = isTOPBOT,
                    IsActiveTOP = true,
                    IsActiveBOT = isTOPBOT,
                    StartStatusTOP = false,
                    StartStatusBOT = false,
                    NameProtocol = ProtocolName,
                    Line = line,
                    //ProgrammName = CreateOrder.ProgrammName,
                };

                fas.EP_Protocols.Add(ep);
                fas.SaveChanges();                
                ListProtocolID.Add(ep.ID);

                IsTOPBOT = (bool)ep.TOPBOT;
            }
        }
        bool GetTopBot(string name)
        {
            return name == "Одностронняя плата" ? false : true;
        }

        public void  GenerateNameProtocol()
        {
            using (var fas = new FASEntities())
            {
                var ModelID = fas.FAS_GS_LOTs.Select(c => new { Name = c.FULL_LOT_Code, modelid = c.ModelID }).Union(fas.Contract_LOT.Select(c => new { Name = c.FullLOTCode, modelid = (short)c.ModelID })).Where(c => c.Name == Order).Select(c => c.modelid).FirstOrDefault();

                ProtocolName = LOTID.ToString() + "_" + fas.FAS_Models.Where(c=>c.ModelID == ModelID).FirstOrDefault().ModelName
               + "_Protocol №" + CountProtocols;
            }
       
        }

        public void AddInfo(int protocolID)
        {
            using (var fas = new FASEntities())
            {
                var IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c => c.Manufacter == "Цех Сборки").ToList();
                addinfo(IdList);

                IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c => c.Manufacter != "Цех Сборки").ToList();

                foreach (var item in new List<string>() { "TOP", "BOT" })
                    addinfo(IdList, item);


                void addinfo(List<EP_TypeVerification> list, string TOP = "TOP")
                {
                    //bool vis = true;

                    //if (TOP == "BOT")                 
                    //    vis = IsTOPBOT == true ? true : false;
                  

                    foreach (var item in list)
                    {
                        EP_ProtocolsInfo INFO = new EP_ProtocolsInfo()
                        {
                            TypeVerifID = item.ID,
                            DateCreate = DateTime.UtcNow.AddHours(2),
                            ProtocolID = protocolID,
                            Signature = false,
                            TOPBOT = TOP,
                            Visible = false,
                            Itter = 1,
                        };

                        fas.EP_ProtocolsInfo.Add(INFO);
                    }

                    fas.SaveChanges();
                }
            }
        }
    }
}