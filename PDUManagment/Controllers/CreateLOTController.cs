using PDUManagment.Classes;
using PDUManagment.Classes.Create;
using PDUManagment.Models;
using PDUManagment.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

            //create.TypeClient = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "N_ВЛВ" }, new SelectListItem() { Text = "Контрактное" } };
            create.TOPBOT = ListTOPBOT;
            create.Client = new List<SelectListItem>() { new SelectListItem() { Text = "" } };
            create.ListModel = new List<SelectListItem>() { new SelectListItem() { Text = ""} };
        

            List<string> ListClients = new List<string>();

            using (var fas = new FASEntities())
            {
                 ListClients = fas.CT_Сustomers.OrderByDescending(c=>c.ID).Select(c => c.СustomerName).Distinct().ToList();
            }

            foreach (var item in ListClients)
                    create.Client.Add(new SelectListItem() { Text = item });

            Session["CreateOrder"] = create;
            return View(create);
           
        }

        public JsonResult GetListModels(string Name)
        {
            var fas = new FASEntities();
            var list = fas.FAS_Models.Where(b => fas.CT_Сustomers.Where(c => c.СustomerName == Name).Select(c => c.ID).FirstOrDefault() == b.CustomerID).Select(b => b.ModelName).ToList();

            var selectlist = new List<SelectListItem>();
            foreach (var item in list) selectlist.Add(new SelectListItem() { Text = item, Value = item});

            return Json(selectlist, JsonRequestBehavior.AllowGet);
        }

       
        [PreventSpam(ErrorMessage = "")]
        public ActionResult CreateLot(CreateOrder create)                
        {            
            // Thread.Sleep(1000);
            //return RedirectToAction("Index");

            if (@Session["Name"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Ошибка";
                Session["CreateOrder"] = create;
                return RedirectToAction("Index");
            }         

            if (create.Count < 1)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Минимальное значение кол-во в заказе 10";
                Session["CreateOrder"] = create;
                return RedirectToAction("Index");
            }

            if (create.LotCode > 32767 || create.LotCode < -32768)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Ошибка";
                Session["CreateOrder"] = create;
                return RedirectToAction("Index");
            }

            var _listDocs = new List<DocumentFile>() { new DocumentFile() { Files = create.BOM, Name = "BOM" } , new DocumentFile() { Files = create.Gerbers, Name = "Gerbers" }

            , new DocumentFile() { Files = create.PickPlace, Name = "PickPlace" } ,  new DocumentFile() { Files = create.AssemblyDrawings, Name = "AssemblyDrawings" } , new DocumentFile() { Files = create.Schematic, Name = "Schematic" }
            ,  new DocumentFile() { Files = create.Fireware, Name = "Firmware" }, new DocumentFile() { Files = create.BlankOrder, Name = "BlankOrder" }};

            create.UserID = int.Parse(Session["UsID"].ToString()); //Записываем userID в переменную

            create.ClientType = create.ClientName == "N_ВЛВ" ? "N_ВЛВ" : "Контрактное";

            var Object = List.Where(c => c.Name == create.ClientType).FirstOrDefault(); //Определяем тип дочернего класса по выбранному типу (ВЛВ, контрактное)
            create.LOTID = Object.AddLot(create); //записываем в переменную LOTID

            //create.IdentifyClient(); //Идентифицируем тип клиента
            create.CheckFolder(); //Проверяем по пути наличие папок, в противном случае создаем их
            create.SaveSpec(); //Сохраняем спецификацию в указнный путь
            create.SaveDoc(_listDocs); //Сохраняем документ, если он был указан по указанному пути
            create.GenerateNameProtocol(); //генерируем имя протокола
            //create.ListProtocolID = new List<int>();
            //foreach (var item in new List<byte>() { 1, 2, 3, 4 }) create.CreateProtocol(item);
            create.CreateProtocol();

            foreach (var item in create.ListDoc) create.CreateDocument(item);

            create.GeneratePGName();
            create.AddInfo();
            create.SetLOG();

            SendMessage(create);

            #region
            //=============================================================================== Отправка письма

            //var listDocs = new FASEntities().EP_Doc.Where(b => b.LOTID == create.LOTID & b.Visible == true).Select(c => new { NameDoc = c.Name, PathDoc = c.Path }).ToList();
            //string linelinks = "";
            //foreach (var item in listDocs)
            //{
            //    var _path = item.PathDoc;
            //    _path = _path.Substring(0, _path.LastIndexOf("\\"));
            //    linelinks += $@"<div> {item.NameDoc}:  <a href=""{_path}""> {item.PathDoc} </a> </div>";
            //}

            //var listPGName = new FASEntities().EP_PGName.Where(b => b.IDProtocol == create.ProtocolID & b.Visible == true).Select(b => new
            //{
            //    Type = b.Type,
            //    Name = b.Name,
            //    Machine = b.EP_Machine.Name,

            //}).ToList();

            //string PGName = "";

            //foreach (var item in listPGName.OrderBy(c => c.Type))
            //{
            //    PGName += $@"                              
            //                <tr>
            //                  <td>  {item.Machine} </td>
            //                  <td>  {item.Type} </td>
            //                  <td>  {item.Name}  </td>
            //                </tr>";
            //}


            //Email.RunEmailFAS($"РАЗМЕЩЁН ЗАКАЗ {create.Order} {create.Date.ToString("MMMM")} {create.Count}шт.", $@"
            //<style>
            //th{{
            //  border-bottom: 0.5px dashed gray
            //}}

            //td{{
            //  padding:1%;
            //  border-left: 1px solid gray;
            //  border-top: 1px solid gray;
            //}}

            //table{{
            //  background-color: aliceblue;
            //}}
            //</style>
            //                     <div>
            //                        <div><h2> Добрый день!</h2> </div>
            //                           <div> <a href=""http://192.168.178.99/ManagmentPDU"">Ссылка Веб-интерфейс с заказами</a></div>
            //                           <div> <a href=""http://192.168.178.99/ElectroniProtocol"">Ссылка Веб-интерфейс с электронными протоколами</a> </div>
            //                        <div><h3> Размещен новый заказ, прошу ознакомиться:</h3>
            //                        {linelinks}
            //                        <div>
            //                          -----------------------------------
            //                          <h1>Список программ оборудования SMT</h1>
            //                             <table>  
            //                            <tr>
            //                              <th colspan= ""2""> </th>
            //                            </tr>
            //                            <tr>
            //                              <th>Оборудование</th>
            //                              <th>Сторона платы</th>
            //                              <th>Имя программы</th>
            //                            </tr>
            //                            {PGName}
            //                            </table>
            //                        </div>
            //                    </div> ");
            #endregion

            return RedirectToAction("Index", "Work");
        }

        void SendMessage(CreateOrder create)
        {
            var fas = new FASEntities();

            var listDocs = new FASEntities().EP_Doc.Where(b => b.LOTID == create.LOTID & b.Visible == true).Select(c => new { NameDoc = c.Name, PathDoc = c.Path }).ToList();
            string linelinks = "";
            foreach (var item in listDocs)
            {
                var _path = item.PathDoc;
                _path = _path.Substring(0, _path.LastIndexOf("\\"));
                linelinks += $@"<div> {item.NameDoc}:  <a href=""{_path}""> {item.PathDoc} </a> </div>";
            }

            var listPGName = fas.EP_PGName.Where(b => b.EP_Protocols.LOTID == create.LOTID & b.Visible == true)
                .GroupBy(c=> new { line = c.EP_Protocols.Line, TOPBOT = c.Type, lotid = c.EP_Protocols.LOTID }).Select(b => new MesPGName()
                {
                    Line = b.Key.line,
                    TOPBOT = b.Key.TOPBOT,
                    LinePGGname = fas.EP_PGName.Where(c=>c.EP_Protocols.Line == b.Key.line & c.Type == b.Key.TOPBOT  & c.EP_Protocols.LOTID == b.Key.lotid).Select(c=> new LinePGGname() {
                    
                        Name = c.Name,
                        PGName = c.EP_Machine.Name,
                    
                    }).ToList(),

                }).ToList();

            string PGName = "";

            foreach (var item in listPGName)
            {
                string data = "";
                foreach (var ii in item.LinePGGname)  data += GetData(ii);                

                PGName += $@"
                <div style = ""margin-top: 1%"">
                  <h3>Линия {item.Line}| Сторона {item.TOPBOT}</h3>
                  <table>  
                       <tr>
                         <th colspan= ""2""> </th>
                       </tr>
                       <tr>
                         <th>Оборудование</th>                        
                         <th>Имя программы</th>
                        </tr>
                       {data}
                    </table> 
                </div>";
            }
           
            
            string GetData(LinePGGname linePG)
            {
                return $@" <tr>
                        <td>  {linePG.PGName} </td>                        
                        <td>  {linePG.Name}  </td>
                        </tr> ";
            }


            Email.RunEmailFAS($"РАЗМЕЩЁН ЗАКАЗ {create.Order} {create.Date.ToString("MMMM")} {create.Count}шт.", $@"
            <style>
            th{{
              border-bottom: 0.5px dashed gray
            }}

            td{{
              padding:1%;
              border-left: 1px solid gray;
              border-top: 1px solid gray;
            }}

            table{{
              background-color: aliceblue;
            }}
            </style>
                                 <div>
                                    <div><h2> Добрый день!</h2> </div>
                                       <div> <a href=""http://192.168.178.99/ManagmentPDU"">Ссылка Веб-интерфейс с заказами</a></div>
                                       <div> <a href=""http://192.168.178.99/ElectroniProtocol"">Ссылка Веб-интерфейс с электронными протоколами</a> </div>
                                    <div><h3> Размещен новый заказ, прошу ознакомиться:</h3>
                                    {linelinks}
                                    <div>
                                      -----------------------------------
                                      <h1>Список программ оборудования SMT</h1>
                                        
                                      {PGName}
                                        
                                    </div>
                                </div> ");

            
        }
    }
}