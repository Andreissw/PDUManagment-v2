﻿using PDUManagment.Classes;
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
            var ListDocs = fas.EP_Doc.Where(c => c.LOTID == ID & c.Visible == true).Select(c => new Document { Name = c.Name, Path = c.Path, ContentType = c.ContentType, NameFile = c.NameFile, ID = c.ID , Extension = c.extension}).ToList();

            CreateOrder create = new CreateOrder()
            {
                //ProtocolID = protocol.ID,
                OTK = IsOTK(),
                ObjectiveFAS = fas.FAS_Objective.Where(c => c.LOTID == protocol.LOTID & c.Manuf == "Цех Сборки").Select(c => c.Objective).FirstOrDefault(),
                ObjectiveSMT = fas.FAS_Objective.Where(c => c.LOTID == protocol.LOTID & c.Manuf == "Цех Поверхностного монтажа").Select(c => c.Objective).FirstOrDefault(),
                ObjectiveGeneral = fas.FAS_Objective.Where(c => c.LOTID == protocol.LOTID & c.Manuf == "Общий заводской").Select(c => c.Objective).FirstOrDefault(),
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

        bool IsOTK()
        {
            var rfid = Session["RFID"].ToString();
            return fas.FAS_Users.Where(c=>c.IDService == 2).Where(c=> c.RFID ==  rfid).Any();
        }


        public ActionResult CloseOpenLot(CreateOrder create)
        {
            var result = fas.Contract_LOT.Where(c => c.ID == create.LOTID).FirstOrDefault();
            if (result != null)
            {
                result.IsActive = !create.IsActive;                
                fas.SaveChanges();
                var lin = result.IsActive == false ? "Закрытие заказа" : "Открытие заказа";
                SetLog(lin, 13, create.LOTID);
                Email.RunEmailFAS($"{lin} {result.FullLOTCode} -> " +
                $"{create.Order}", $"Добрый день! Закрыт заказ: {result.FullLOTCode} .</br> Внёс изменения пользователь: {Session["Name"]}");
                return RedirectToAction("Index", new { ID = create.LOTID });
            }

            var Result = fas.FAS_GS_LOTs.Where(c => c.LOTID == create.LOTID).FirstOrDefault();
            if (Result == null)
            {         
                return RedirectToAction("Index", new { ID = create.LOTID });
            }            

            Result.IsActive = !create.IsActive;
            var line = Result.IsActive == false ? "Закрытие заказа" : "Открытие заказа";
            SetLog(line, 13, create.LOTID);
            fas.SaveChanges();

            Email.RunEmailFAS($"{line} {Result.FULL_LOT_Code} -> " +
            $"{create.Order}", $"Добрый день! Закрыт заказ: {Result.FULL_LOT_Code} .</br> Внёс изменения пользователь: {Session["Name"]}");

            return RedirectToAction("Index", new { ID = create.LOTID  });       
        }

        public FileResult Download(string path,string ContentType, string Name, string Extension)
        {
            var doc = new byte[0];

            try
            {
                doc = System.IO.File.ReadAllBytes(path);
            }
            catch (Exception)
            {

                return File(new byte[0], "application/octet-stream", "ФайлУдалёнИлиНеНайден.txt");
            }
           

            if (ContentType == null)        
                ContentType = "application/octet-stream";             
          
            //var contentType = fas.EP_Doc.Where(c => c.Path == path).Select(c => c.ContentType).FirstOrDefault();
            //var Name = fas.EP_Doc.Where(c => c.Path == path).Select(c => c.NameFile).FirstOrDefault();
            return File(doc, ContentType, Name + Extension);
        }
       
        [HttpPost]
        public ActionResult EditOrder(CreateOrder create)
        {
            if (@Session["Name"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = fas.Contract_LOT.Where(c => c.ID == create.LOTID).FirstOrDefault();
            
            if (result != null)
            {
                var ord = result.FullLOTCode;
                result.FullLOTCode = create.Order;
                TempData["OKOrder"] = "Сохранено";
                SetLog(ord +" - "+ create.Order,7,create.LOTID);
                fas.SaveChanges();

                Email.RunEmailFAS($"Изменение имени заказа: {ord} -> " +
              $"{create.Order}", $"Добрый день! Изменено имя заказа.</br> Старое именование: {ord}.</br> Новое Именование: {create.Order}.</br> Внёс изменения пользователь: {Session["Name"]}");

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

            Email.RunEmailFAS($"Изменение имени заказа {orde} -> " +
                $"{create.Order}",$"Добрый день! Изменено имя заказа.</br> Старое именование: {orde}.</br> Новое Именование: {create.Order}.</br> Внёс изменения пользователь: {Session["Name"]}");

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

                Email.RunEmailFAS($"Изменение количество - {create.Count} в заказе: {result.FullLOTCode}"
              , $"Добрый день! Измененно количество в заказе: {result.FullLOTCode}.</br> Старое количество: {c}.</br> Новое количество: {create.Count}.</br> Внёс изменения пользователь: {Session["Name"]} ");

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

            Email.RunEmailFAS($"Изменение количество - {create.Count} в заказе: {result.FullLOTCode}"
                ,$"Добрый день! Измененно количество в заказе: {result.FullLOTCode}.</br> Старое количество: {co}.</br> Новое количество: {create.Count}.</br> Внёс изменения пользователь: {Session["Name"]} ");

            return RedirectToAction("Index", new { ID = create.LOTID });            
        }

        public ActionResult EditTOPBOT(CreateOrder create)
        {
            var obj = fas.EP_Protocols.Where(c => c.ID == create.LOTID);        
            var topbot = obj.FirstOrDefault().TOPBOT;
            obj.FirstOrDefault().TOPBOT = create.TOPBOTName == "Одностронняя плата" ? false : true;
            TempData["OKTOP"] = "Сохранено";
            SetLog(topbot + " - " + create.TOPBOTName,9, create.LOTID);
            fas.SaveChanges();

            //Email.RunEmailFAS($"Изменение стороны - {create.TOPBOT} в протоколе {obj.FullLOTCode}"
            //, $"Добрый день! Измененно количество в заказе {result.FullLOTCode}. Старое количество {result.LOTSize}. Новое количество {create.Count}. Внёс изменения пользователь {Session["Name"]} ");

            return RedirectToAction("Index", new { ID = create.LOTID });
        }

      

        public JsonResult RemoveDocument(int ID)
        {
            var model = fas.EP_Doc.Where(c => c.ID == ID).FirstOrDefault();
            var lotid = fas.EP_Doc.Where(c => c.ID == ID).Select(c => c.LOTID).FirstOrDefault();

            CreateOrder createClass = new CreateOrder();
            var newpath = createClass.CheckFolderArchive(model.Path);
            model.Visible = false;
            if (newpath.Contains("FAIL")) { fas.SaveChanges(); return Json(newpath, JsonRequestBehavior.AllowGet); }

            model.Path = newpath;

            SetLog("", 11, ID, lotid);
            fas.SaveChanges();

            var view = GetSpecName(model.LOTID);

            Email.RunEmailFAS($"Удаление документа - {model.NameFile} в заказе: {view.NameOrder}"
            , $"Добрый день! удалён документ:  {model.Name} {model.NameFile}.</br> Внёс изменения пользователь: {Session["Name"]} ");

            return Json(true ,JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetObjective(int LOTID,double Objective, string Type)
        {
            if (GetObjective(LOTID,Type))
                UpateObj(LOTID, Objective, Type);
            else
                SetObj(LOTID,Objective,Type);

            var data = GetSpecName(LOTID);

            Email.RunEmailFAS("Изменение цели по качеству", $@"Цель по заказу {data.NameOrder}. Цех: {Type} изменена на {Objective}");

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        void SetObj(int LOTID, double Objective, string Type)
        {
            FAS_Objective fAS_Objective = new FAS_Objective()
            {
                LOTID = LOTID,
                Objective = Objective,
                Manuf = Type,
            };

            fas.FAS_Objective.Add(fAS_Objective);
            fas.SaveChanges();
        }

        void UpateObj(int LOTID, double Objective,string Type)
        {
            var result = fas.FAS_Objective.Where(c => c.LOTID == LOTID & c.Manuf == Type);
            result.FirstOrDefault().Objective = Objective;
            fas.SaveChanges();
        }

        bool GetObjective(int LOTID, string Type)
        {
            return fas.FAS_Objective.Where(c => c.LOTID == LOTID & c.Manuf == Type).Select(c => c.ID == c.ID).FirstOrDefault();
        }

        public JsonResult RenameDocument(int ID, string name)
        {
            var model = fas.EP_Doc.Where(c => c.ID == ID).FirstOrDefault();
            var lotid = fas.EP_Doc.Where(c => c.ID == ID).Select(c => fas.EP_Protocols.Where(b => b.ID == c.IDProtocol).Select(b => b.LOTID).FirstOrDefault()).FirstOrDefault();
            var n = model.NameFile;
            model.NameFile = name;
            SetLog(n + " - " + name, 12, ID ,lotid);
            fas.SaveChanges();

            var view = GetSpecName(model.LOTID);

            Email.RunEmailFAS($"Изменено имя документа - {model.Name} в заказе: {view.NameOrder}"
            , $"Добрый день! Изменено имя документа.</br>Старое имя: {n}. </br>Новое имя: {name}. </br>Внёс изменения пользователь: {Session["Name"]} ");

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddDocs(CreateOrder create)
        {
            
            var view = GetSpecName(create.LOTID);
            create.Date = view.DateManufacter;
            var _listDocs = new List<DocumentFile>() { new DocumentFile() { Files = create.BOM, Name = "BOM" } , new DocumentFile() { Files = create.Gerbers, Name = "Gerbers" }

           , new DocumentFile() { Files = create.PickPlace, Name = "PickPlace" } ,  new DocumentFile() { Files = create.AssemblyDrawings, Name = "AssemblyDrawings" } , new DocumentFile() { Files = create.Schematic, Name = "Schematic" }
           ,  new DocumentFile() { Files = create.Fireware, Name = "FirmWare" }, new DocumentFile() { Files = create.BlankOrder, Name = "BlankOrder" } };
            create.ClientName = create.ClientName == "ВЛВ"? "N_ВЛВ" : create.ClientName;
            create.CheckFolder();
            create.SaveDoc(_listDocs);

            if (create.FileSpec != null)
                create.SaveSpec();


            foreach (var item in create.ListDoc) { 
                var idDoc = create.CreateDocument(item);
                SetLog("", 10, idDoc,create.LOTID);
            }

            Email.RunEmailFAS($"Добавлены документы в заказе: {view.NameOrder}"
           , $"Добрый день! Добавлены следующие документы. </br> {string.Join(",", create.ListDoc.Select(c => c.NameFile))}.</br> Внёс изменения пользователь: {Session["Name"]} ");

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
                DateManufacter = (DateTime)c.DateManufacter,

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
                    DateManufacter = (DateTime)c.DateManufacter,

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