using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using PromoServicesApplication.Models;
using System.Net.Mail;
using System.Web.Helpers;

namespace PromoServicesApplication.Controllers
{
    public class HomeController : Controller
    {        
        public ActionResult Index()
        {
            ViewBag.Msg = ViewBag.Message;
            return View();
        }

        [HttpPost]
        public ActionResult Index(EventTableProd a)
        {
            a.Type = Request.Form["type_selected"];
            a.Music = Request.Form["music_selected"];
            a.State = Request.Form["state_selected"];
            a.Description = HttpUtility.HtmlEncode(Request.Form["Description"]);
            using (var binaryReader = new BinaryReader(Request.Files["FlyerFile"].InputStream))
            {
                a.Flyer = binaryReader.ReadBytes(Request.Files["FlyerFile"].ContentLength);
            }
            string eventUrl = string.Empty;
            YPHModels.Post(a, out eventUrl);
            EmailModel.SendEventPostedMail(a, eventUrl);
            //EventsEntities tt = new EventsEntities();
            //tt.EventTableProds.Add(a);
            //tt.SaveChanges();
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = string.Empty;

            return View("Index");
        }

        [HttpPost]
        public ActionResult Contact(InquiryTable i)
        {
             if (EmailModel.SendMail2(i))
            {
                i = new InquiryTable();
                ViewBag.Message = "Inquiry Received!";
                return View("Index");
            }
            ViewBag.Message = "Oops! Something went wrong. Please try submitting your inquiry again.";
            return RedirectToAction("Index");
        }

        
    }
}