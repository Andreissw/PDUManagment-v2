using PDUManagment.Classes;
using PDUManagment.Classes.Create;
using PDUManagment.Models;
using PDUManagment.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Controllers
{
    public class CreateLOTController : Controller
    {
        // GET: CreateLOT
        List<SelectListItem> ListTOPBOT = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "Одностронняя плата" }, new SelectListItem() { Text = "Двусторонняя плата" }, };
        List<BaseClass> List = new List<BaseClass>() { new GS(), new Contract() };

      
        public ActionResult Index()      
        {
            CreateOrder create = new CreateOrder();
            if (Session["CreateOrder"] != null)
            {
                create = (CreateOrder)Session["CreateOrder"];
                create.SpecificationBom = string.Empty;               
            }

            create.TypeClient = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "ВЛВ" }, new SelectListItem() { Text = "Контрактное" } };
            create.TOPBOT = ListTOPBOT;
            create.Client = new List<SelectListItem>() { new SelectListItem() { Text = "" } };
        

            List<string> ListClients = new List<string>();

            using (var fas = new FASEntities())
            {
                 ListClients = fas.CT_Сustomers.Select(c => c.СustomerName).Distinct().ToList();
            }

            foreach (var item in ListClients)
                    create.Client.Add(new SelectListItem() { Text = item });

            Session["CreateOrder"] = create;
            return View(create);
           
        }


        [PreventSpam(ErrorMessage = "Ошибка")]
        public ActionResult CreateLot(CreateOrder create)
        {            
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Ошибка";
                Session["CreateOrder"] = create;
                return RedirectToAction("Index");
            }

            if (create.Count < 10)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Минимальное значение кол-во в заказе 10";
                Session["CreateOrder"] = create;
                return RedirectToAction("Index");
            }

           var _listDocs = new List<DocumentFile>() { new DocumentFile() { Files = create.BOM, Name = "BOM" } , new DocumentFile() { Files = create.Gerbers, Name = "Gerbers" }

           , new DocumentFile() { Files = create.PickPlace, Name = "PickPlace" } ,  new DocumentFile() { Files = create.AssemblyDrawings, Name = "AssemblyDrawings" } , new DocumentFile() { Files = create.Schematic, Name = "Schematic" }
           ,  new DocumentFile() { Files = create.Fireware, Name = "Firmware" } };

            create.UserID = int.Parse(Session["UsID"].ToString()); //Записываем userID в переменную
            var Object = List.Where(c => c.Name == create.ClientType).FirstOrDefault(); //Определяем тип дочернего класса по выбранному типу (ВЛВ, контрактное)
            create.LOTID = Object.AddLot(create); //записываем в переменную LOTID

            create.IdentifyClient(); //Идентифицируем тип клиента
            create.CheckFolder(); //Проверяем по пути наличие папок, в противном случае создаем их
            create.SaveSpec(); //Сохраняем спецификацию в указнный путь
            create.SaveDoc(_listDocs); //Сохраняем документ, если он был указан по указанному пути
            create.GenerateNameProtocol(); //генерируем имя протокола
            create.CreateProtocol();

            foreach (var item in create.ListDoc)           
                create.CreateDocument(item);        
            
            create.GeneratePGName();
            create.AddInfo();
            create.SetLOG();

            Email.RunEmailFAS($"Создан Заказ: {create.Order}", $@"<div><h2> Добрый день!</h2> </div>
                        <div><h2> Создан новый заказ {create.Order}</h2></div> ");

            return RedirectToAction("Index","Work");
        }
    }
}