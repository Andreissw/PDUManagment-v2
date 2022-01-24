using PDUManagment.Classes.Create;
using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDUManagment.Classes
{
    public abstract class BaseClass
    {
        readonly public FASEntities fas;

        public BaseClass()
        {
            fas = new FASEntities();
        }

        public string Name { get; set; }        
        public abstract int AddLot(CreateOrder create);

        public abstract void UpdateLot( EditLot editLot);

        public void UpdateProtocol(EditLot editLot)
        {
            var Result = fas.EP_Protocols.Where(c => c.ID == editLot.ProtocolID).FirstOrDefault();
          
            Result.TOPBOT = editLot.TOPBOTName == "TOP" ? false: true;
        }

        public void UpdateVisibleTOPBOT(EditLot editLot)
        {
            var result = editLot.TOPBOTName == "TOP" ? false : true;

            var obj = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == editLot.ProtocolID & c.TOPBOT == "BOT").AsEnumerable()
                .Select(c => { c.Visible = result; return c; });

            foreach (var item in obj)          
                fas.Entry(item).State = (System.Data.Entity.EntityState)System.Data.Entity.EntityState.Modified;           

            fas.SaveChanges();
        }

        public void SetLog(int UserID, string Mes)
        {
            EP_Log log = new EP_Log()
            {
                UserID = (short)UserID,
                ServiceID = GetServiceID(UserID),                
                Description = Mes,
            };

            fas.EP_Log.Add(log);
            fas.SaveChanges();
        }

        int GetServiceID(int UserID)
        {
            return (int)fas.FAS_Users.Where(c => c.UserID == UserID).FirstOrDefault().IDService;
        }

    }

    public class GS : BaseClass
    {

        public GS() 
        {
            this.Name = "N_ВЛВ";            
        }

        public override int AddLot(CreateOrder create)
        {
            var modelid = fas.FAS_Models.Where(c => c.ModelName == create.Modelname).Select(c => c.ModelID).FirstOrDefault();

            FAS_GS_LOTs fAS_GS_LOTs = new FAS_GS_LOTs()
            {
                FULL_LOT_Code = create.Order,
                CreateByID = (short)create.UserID,
                CreateDate = DateTime.UtcNow.AddHours(2),
                Specification = create.SpecificationBom,
                CountinLot = create.Count,
                IsActive = true,
                DateManufacter = create.Date,
                ModelID = modelid,
                LOTCode = (short)create.LotCode,
                Manufacturer = "DTVS",
                Operator = "TRICOLOR",

               
            };

            fas.FAS_GS_LOTs.Add(fAS_GS_LOTs);
            fas.SaveChanges();
            return fas.FAS_GS_LOTs.OrderByDescending(c => c.CreateDate).Select(c => c.LOTID).FirstOrDefault();


            //return 30;
        }

        public override void UpdateLot( EditLot editLot)
        {
            var Result = fas.FAS_GS_LOTs.Where(c => c.LOTID == editLot.LOTID).FirstOrDefault();
            Result.FULL_LOT_Code = editLot.Order;
            Result.Specification = editLot.SpecBOM;
        }
    }

    public class Contract : BaseClass
    {        
        public Contract()
        {
            this.Name = "Контрактное";            
        }

        public override int AddLot(CreateOrder create )
        {

            var CustomersID = fas.CT_Сustomers.Where(c => c.СustomerName == create.ClientName).FirstOrDefault().ID;
            var modelid = fas.FAS_Models.Where(c => c.ModelName == create.Modelname).Select(c => c.ModelID).FirstOrDefault();

            Contract_LOT contract_LOT = new Contract_LOT()
            {
                FullLOTCode = create.Order,
                СustomersID = CustomersID,
                CreateDate = DateTime.UtcNow.AddHours(2),
                Specification = create.SpecificationBom,
                LOTSize = create.Count,
                IsActive = true,
                DateManufacter = create.Date,
                ModelID = modelid,
                LOTCode = (short)create.LotCode,
                
            };

            fas.Contract_LOT.Add(contract_LOT);
            fas.SaveChanges();
            return fas.Contract_LOT.OrderByDescending(c => c.CreateDate).Select(c => c.ID).FirstOrDefault();


            //return 20065;
        }

        public override void UpdateLot(EditLot editLot)
        {
            var Result = fas.Contract_LOT.Where(c => c.ID == editLot.LOTID).FirstOrDefault();
            Result.FullLOTCode = editLot.Order;
            Result.Specification = editLot.SpecBOM;

        }
    }

}