using PDUManagment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PDUManagment.Controllers
{
    public class HomeController : Controller
    {
        FASEntities fas = new FASEntities();
        public ActionResult Index()
        {
            HttpContext.Session["userID"] = "1";
            Session["RFID"] = null;
            Session["Name"] = null;
            Session["UsID"] = null;
            return View();
        }

        public ActionResult GetUserID(User user, bool Loggin)
        {
            if (!Loggin)
            {
                Session["Service"] = 0;
                Session["RFID"] = 0;
                Session["Name"] = 0;
                Session["UsID"] = 0;
                return RedirectToAction("Index", "Work");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Не верный формат пароля";
                return RedirectToAction("Index");
            }
           
            var ListUser = PDUManagment.User.CheckUser(user.RFID);

            if (ListUser.Count == 0)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Не верный пароль";
                return RedirectToAction("Index");
            }

            //if (ListUser[1] != user.IDService.ToString())
            //{
            //    ModelState.AddModelError("", "Не верно введены данные");
            //    TempData["Er"] = "Недостаточно прав!";
            //    return RedirectToAction("Index");
            //}

            TempData["Er"] = null;
            user.Name = ListUser.FirstOrDefault();
            user.UserID = fas.FAS_Users.Where(c => c.UserName == ListUser.FirstOrDefault()).Select(c => c.UserID).FirstOrDefault();
            Session["Service"] = ListUser[1];
            Session["RFID"] = user.RFID;
            Session["Name"] = ListUser[0];
            Session["UsID"] = user.UserID;
            return RedirectToAction("Index","Work");
        }
   
    }
}