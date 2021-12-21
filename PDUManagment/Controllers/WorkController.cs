using Microsoft.AspNetCore.Mvc;
using PDUManagment.Classes;
using PDUManagment.Classes.Create;
using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Controllers
{
    public class WorkController : Controller
    {
        // GET: Work

        FASEntities fas = new FASEntities();
        List<BaseClass> List = new List<BaseClass>() { new GS(), new Contract() };
        List<SelectListItem> ListTOPBOT = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "TOP" }, new SelectListItem() { Text = "TOP/BOT" }, };

        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult GetPGName(int ProtocolID, string name, string Protocolname)
        {
            List<ProgrammNames> programmNames = new List<ProgrammNames>();

            programmNames = fas.EP_PGName.Where(c => c.IDProtocol == ProtocolID & c.Visible == true).Select(c => new ProgrammNames
            {
                ProtocolName = Protocolname,
                Name = name,
                TOPBOT = c.Type,
                Machine = fas.EP_Machine.Where(b => b.ID == c.IDMachine).FirstOrDefault().Name,
                PGName = c.Name,

            }).ToList();

            return View(programmNames);
        }

        public ActionResult GetTableLots()
        {
            List<Table> tables = new List<Table>();

            var ListLots = fas.EP_Protocols.OrderByDescending(c => c.DateCreate).Select(c => c.LOTID).Distinct().ToList();

            foreach (var item in ListLots)
            {
                var dataView = GetSpecName(item);
                Table table = new Table()
                {
                     ID = item,
                     NameClient = dataView.NameClient,
                     Name = dataView.NameOrder,
                     DateCreate = dataView.DateCreate,
                     TypeClient = dataView.ClientType,            
                     IsActive = dataView.IsActive,
                };

                tables.Add(table);
            }

            return View(tables);
        }
       
        public ActionResult GetTableProtocols(int LOTID)
        {
            List<Table> tables = new List<Table>();

            var listProtocols = fas.EP_Protocols.Where(c=>c.LOTID == LOTID).OrderByDescending(c=>c.DateCreate).Select(c => new { ID = c.ID, DateCreate = c.DateCreate, LOTID = (int)c.LOTID, TOPBOT = c.TOPBOT, nameprt = c.NameProtocol }).ToList();

            foreach (var item in listProtocols)
            {
                var dataView = GetSpecName(item.LOTID);
                Table table = new Table()
                {
                    ID = item.ID,
                    DateCreate = (DateTime)item.DateCreate,
                    ProtocolName = item.nameprt,
                    ProgrammName = fas.EP_PGName.Where(b=>b.IDProtocol == item.ID).Select(b=> new  ProgrammNames                    
                    { 
                        Machine =  b.EP_Machine.Name,                       
                        PGName = b.Name

                    }).ToList(),

                    TOPBOT = item.TOPBOT == true ? "Двусторонняя плата" : "Одностороняя плата",
                    TypeClient = dataView.ClientType,
                    Name = dataView.NameOrder,
                    Spec = dataView.NameSpec,
                    NameClient = dataView.NameClient
                };
                tables.Add(table);

            }

            return View(tables);
        }

        ViewData GetSpecName(int LOTID)
        {
            ViewData viewData = new ViewData();

            viewData = fas.FAS_GS_LOTs.Where(c => c.LOTID == LOTID).Select(c => new ViewData

            {
                NameSpec = c.Specification,
                NameOrder = c.FULL_LOT_Code,
                ClientType = "ВЛВ",
                NameClient = "ВЛВ",
                DateCreate = c.CreateDate,
                IsActive = c.IsActive,

            }).FirstOrDefault();

            if (viewData == null)
            {
                viewData = fas.Contract_LOT.Where(c => c.ID == LOTID).Select(c => new ViewData
                {
                    NameSpec = c.Specification,
                    NameOrder = c.FullLOTCode,
                    ClientType = "Контрактное",
                    NameClient = fas.CT_Сustomers.Where(b => b.ID == c.СustomersID).FirstOrDefault().СustomerName,
                    DateCreate = c.CreateDate,
                    IsActive = c.IsActive,

                }).FirstOrDefault();
            }
            return viewData;
        }





    }
}