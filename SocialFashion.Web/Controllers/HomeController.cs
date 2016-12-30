using Microsoft.AspNet.Identity;
using SocialFashion.Model.Models;
using SocialFashion.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialFashion.Web.Controllers
{
    [Authorize(Roles = "user")]
    public class HomeController : Controller
    {
        SocialFashionDbContext db;
        public HomeController(IProductService productServicer)
        {
            db = new SocialFashionDbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Chat()
        {
            return PartialView();
        }

        public ActionResult ToolbarDesktopPartial()
        {
            return PartialView();
        }

        public ActionResult SearchUsersByKeyWord()
        {
            return PartialView();
        }

        public JsonResult GetNotificationAPI()
        {

            var currentUserId = User.Identity.GetUserId();
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                List<Notifications_GetNotiByRecieverId_Result> result = db.Notifications_GetNotiByRecieverId(currentUserId).ToList();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public JsonResult GetCountNotificationAPI()
        {

            var currentUserId = User.Identity.GetUserId();
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                int? result = db.Notifications_GetCountNotiByRecieverId(currentUserId).FirstOrDefault();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public JsonResult GetMessageAPI()
        {
            var currentUserMail = User.Identity.GetUserName();
            List<Message> list = db.Messages.Where(x => x.RecieverEmail == currentUserMail).OrderByDescending(x => x.Date).ToList();
            var map = new Dictionary<string, Message>();
            foreach (Message s in list)
            {
                if (!map.ContainsKey(s.SenderEmail))
                {
                    map.Add(s.SenderEmail, s);
                }
            }
            List<Message> listMessage = new List<Message>();
            foreach (KeyValuePair<string, Message> entry in map)
            {
                listMessage.Add(entry.Value);
            }

            return new JsonResult { Data = listMessage, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            

           
        }

        public JsonResult GetCountMessageAPI()
        {

            var currentUserMail = User.Identity.GetUserName();

            List<Message> list = db.Messages.Where(x => x.RecieverEmail == currentUserMail).OrderByDescending(x => x.Date).ToList();
            var map = new Dictionary<string, Message>();
            foreach (Message s in list)
            {
                if (!map.ContainsKey(s.SenderEmail))
                {
                    map.Add(s.SenderEmail, s);
                }
            }
            List<Message> listMessage = new List<Message>();
            foreach (KeyValuePair<string, Message> entry in map)
            {
                listMessage.Add(entry.Value);
            }
            var countMsg = listMessage.Count();
            return new JsonResult { Data = countMsg, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            
        }

        public JsonResult GetFanAPI()
        {
            var currentUserId = User.Identity.GetUserId();
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                List<Fans_GetFanByRequestId_Result>  result = db.Fans_GetFanByRequestId(currentUserId).ToList();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }


        }

        public JsonResult GetCountFanAPI()
        {
            var currentUserId = User.Identity.GetUserId();
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                int? result = db.Fans_GetCountFanByRequestId(currentUserId).FirstOrDefault();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult GetMemberSearchData(string searchString)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                List<AspNetUsers_SearchUserByKey_Result> result = db.AspNetUsers_SearchUserByKey(searchString).ToList();
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }


        }
    }
}