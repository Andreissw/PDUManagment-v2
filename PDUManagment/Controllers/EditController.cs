using PDUManagment.Classes;
using PDUManagment.Classes.Create;
using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Controllers
{
    public class EditController : Controller
    {
        FASEntities fas = new FASEntities();
        List<BaseClass> List = new List<BaseClass>() { new GS(), new Contract() };
        List<SelectListItem> ListTOPBOT = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "Одностронняя плата" }, new SelectListItem() { Text = "Двусторонняя плата" }, };
        // GET: Edit
        
        [HttpGet]
        public ActionResult Index(int ID, string mode)
        {

            var protocol = fas.EP_Protocols.Where(c => c.LOTID == ID).Select(c => new { ID = c.ID, LOTID = (int)c.LOTID, TOPBOT = c.TOPBOT, ProtocolName = c.NameProtocol }).FirstOrDefault();
            var dataView = GetSpecName(protocol.LOTID);
            var listProtocols = fas.EP_Protocols.Where(c => c.LOTID == protocol.LOTID).Select(c => c.ID).ToList();
            var ListDocs = fas.EP_Doc.Where(c => listProtocols.Contains((int)c.IDProtocol) & c.Visible == true).Select(c => new Document { Name = c.Name, Path = c.Path, ContentType = c.ContentType, NameFile = c.NameFile, ID = c.ID , Extension = c.extension}).ToList();

            CreateOrder create = new CreateOrder()
            {
                ProtocolID = protocol.ID,
                mode = mode,
                Order = dataView.NameOrder,
                SpecificationBom = dataView.NameSpec,
                ClientName = dataView.NameClient,
                TOPBOTName = protocol.TOPBOT == true ? "Двусторонняя плата" : "Одностронняя плата",             
                ListDocs = ListDocs,
                TOPBOT = ListTOPBOT,
                LOTID = protocol.LOTID,
                ClientType = dataView.ClientType,
                ProtocolName = protocol.ProtocolName,
                Count = dataView.Count,
                IsActive = dataView.IsActive,
            };
            return View(create);
        }


        public ActionResult CloseOpenLot(CreateOrder create)
        {
            var result = fas.Contract_LOT.Where(c => c.ID == create.LOTID).FirstOrDefault();
            if (result != null)
            {
                result.IsActive = !create.IsActive;                
                fas.SaveChanges();
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            var Result = fas.FAS_GS_LOTs.Where(c => c.LOTID == create.LOTID).FirstOrDefault();
            if (Result == null)
            {         
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            Result.IsActive = !create.IsActive;           
            fas.SaveChanges();
            return RedirectToAction("Index", new { ID = create.LOTID });       
        }

        public FileResult Download(string path,string ContentType, string Name, string Extension)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(path);

            if (ContentType == null)        
                ContentType = "application/octet-stream";             
          
            //var contentType = fas.EP_Doc.Where(c => c.Path == path).Select(c => c.ContentType).FirstOrDefault();
            //var Name = fas.EP_Doc.Where(c => c.Path == path).Select(c => c.NameFile).FirstOrDefault();
            return File(doc, ContentType, Name + Extension);
        }
       
        [HttpPost]
        public ActionResult EditOrder(CreateOrder create)
        {
            var result = fas.Contract_LOT.Where(c => c.ID == create.LOTID).FirstOrDefault();
            
            if (result != null)
            {
                var ord = result.FullLOTCode;
                result.FullLOTCode = create.Order;
                TempData["OKOrder"] = "Сохранено";
                SetLog(ord +" - "+ create.Order,7,create.LOTID);
                fas.SaveChanges();
                return RedirectToAction("Index", new {ID = create.LOTID });
            }

            var Result = fas.FAS_GS_LOTs.Where(c => c.LOTID == create.LOTID).FirstOrDefault();
            if (Result == null)
            {
                TempData["ErrOrder"] = "Не удалось изменить имя заказа";              
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            var orde = Result.FULL_LOT_Code;
            Result.FULL_LOT_Code = create.Order;
            TempData["OKOrder"] = "Сохранено";
            SetLog(orde + " - " + create.Order, 7,create.LOTID);
            fas.SaveChanges();
            return RedirectToAction("Index", new { ID = create.LOTID });
        }

        [HttpPost]
        public ActionResult EditCount(CreateOrder create)
        {
            var result = fas.Contract_LOT.Where(c => c.ID == create.LOTID).FirstOrDefault();
            if (result != null)
            {
                var c = result.LOTSize;
                result.LOTSize = create.Count;
                TempData["OKC"] = "Сохранено";
                SetLog(c.ToString() + " - " + create.Count.ToString(), 8,create.LOTID);
                fas.SaveChanges();
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            var Result = fas.FAS_GS_LOTs.Where(c => c.LOTID == create.LOTID).FirstOrDefault();
            if (Result == null)
            {
                TempData["ErrC"] = "Не удалось изменить имя заказа";
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            var co = Result.CountinLot;
            Result.CountinLot = create.Count;
            TempData["OKC"] = "Сохранено";

            SetLog(co.ToString() + " - " + create.Count.ToString(), 8,create.LOTID);
            fas.SaveChanges();
            return RedirectToAction("Index", new { ID = create.LOTID });            
        }

        public ActionResult EditTOPBOT(CreateOrder create)
        {
            var obj = fas.EP_Protocols.Where(c => c.ID == create.ProtocolID);        
            var topbot = obj.FirstOrDefault().TOPBOT;
            obj.FirstOrDefault().TOPBOT = create.TOPBOTName == "Одностронняя плата" ? false : true;
            TempData["OKTOP"] = "Сохранено";
            SetLog(topbot + " - " + create.TOPBOTName,9, create.LOTID);
            fas.SaveChanges();
            return RedirectToAction("Index", new { ID = create.LOTID });
        }

      

        public JsonResult RemoveDocument(int ID)
        {
            var model = fas.EP_Doc.Where(c => c.ID == ID).FirstOrDefault();
            var lotid = fas.EP_Doc.Where(c => c.ID == ID).Select(c => fas.EP_Protocols.Where(b => b.ID == c.IDProtocol).Select(b => b.LOTID).FirstOrDefault()).FirstOrDefault();
            model.Visible = false;
            SetLog("", 11, ID, lotid);
            fas.SaveChanges();
            return Json(true ,JsonRequestBehavior.AllowGet);
        }

        public JsonResult RenameDocument(int ID, string name)
        {
            var model = fas.EP_Doc.Where(c => c.ID == ID).FirstOrDefault();
            var lotid = fas.EP_Doc.Where(c => c.ID == ID).Select(c => fas.EP_Protocols.Where(b => b.ID == c.IDProtocol).Select(b => b.LOTID).FirstOrDefault()).FirstOrDefault();
            var n = model.NameFile;
            model.NameFile = name;
            SetLog(n + " - " + name, 12, ID ,lotid);
            fas.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddDocs(CreateOrder create)
        {
            var _listDocs = new List<DocumentFile>() { new DocumentFile() { Files = create.BOM, Name = "BOM" } , new DocumentFile() { Files = create.Gerbers, Name = "Gerbers" }

           , new DocumentFile() { Files = create.PickPlace, Name = "PickPlace" } ,  new DocumentFile() { Files = create.AssemblyDrawings, Name = "AssemblyDrawings" } , new DocumentFile() { Files = create.Schematic, Name = "Schematic" }
           ,  new DocumentFile() { Files = create.Fireware, Name = "FirmWare" } };

            create.CheckFolder();
            create.SaveDoc(_listDocs);

            if (create.FileSpec != null)
                create.SaveSpec();


            foreach (var item in create.ListDoc) { 
                var idDoc = create.CreateDocument(item);
                SetLog("", 10, idDoc,create.LOTID);
            }

            return RedirectToAction("Index", new { ID = create.LOTID });
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
                Count = (int)c.CountinLot,
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
                    Count = (int)c.LOTSize,
                    IsActive = c.IsActive,

                }).FirstOrDefault();
            }
            return viewData;
        }

        void SetLog(string mes, int idstep, int idDocument, int LOTID)
        {
            var UserID = int.Parse(Session["USID"].ToString());
            var ServiceID = int.Parse(Session["Service"].ToString());
            EP_Log eP_Log = new EP_Log()
            {
                UserID = (short)UserID,
                ServiceID = ServiceID,
                Description = mes,
                Date = DateTime.UtcNow.AddHours(2),
                IDStep = idstep,
                DocumentID = idDocument,
                LOTID = LOTID,

            };

            fas.EP_Log.Add(eP_Log);
            fas.SaveChanges();
        }

        void SetLog(string mes, int idstep,int LOTID)
        {
            var UserID = int.Parse(Session["USID"].ToString());
            var ServiceID = int.Parse(Session["Service"].ToString());
            EP_Log eP_Log = new EP_Log()
            {
                UserID = (short)UserID,
                ServiceID = ServiceID,
                Description = mes,
                Date = DateTime.UtcNow.AddHours(2),
                IDStep = idstep,    
                LOTID = LOTID,
            };

            fas.EP_Log.Add(eP_Log);
            fas.SaveChanges();
        }

    }
}