using SocialFashion.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SocialFashion.Web.Models;
using System.Threading.Tasks;

namespace SocialFashion.Web.Controllers
{
    [Authorize(Roles = "user")]
    public class UserController : Controller
    {
        private ApplicationUserManager _userManager;
        public UserController()
        {

        }

        public UserController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult TimeLineBackGroundProfile()
        {
            return PartialView();
        }
        // GET: User
        public ActionResult UserProfile(string id)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                var currentUserId = User.Identity.GetUserId();
                if (Object.Equals(currentUserId, id))
                {
                    ViewBag.IsFanOrOwn = 0;
                }
                else
                {
                    var userFan = db.Fans_GetFanByUser(currentUserId, id).FirstOrDefault();

                    if (userFan != null)
                    {
                        if (userFan.SenderId == currentUserId)
                        {
                            ViewBag.FriendStatus = userFan.Status;
                            ViewBag.checkHaveAddFriend = 0;
                        }
                        if (userFan.RequestId == currentUserId)
                        {
                            ViewBag.FriendStatus = userFan.Status;
                            AspNetUsers_CheckAddFriend_Result checkHaveAddFriend = db.AspNetUsers_CheckAddFriend(id).FirstOrDefault();
                            if (checkHaveAddFriend != null)
                            {
                                ViewBag.checkHaveAddFriend = 1;
                            }
                            else
                            {
                                ViewBag.checkHaveAddFriend = 0;
                            }
                        }

                    }
                    else
                    {
                        ViewBag.checkHaveAddFriend = 0;
                        ViewBag.IsAddFan = 1;
                        ViewBag.FriendStatus = -1;
                    }


                    ViewBag.IsFanOrOwn = 1;
                }

                Users_GetById_Result userProfile = db.Users_GetById(id).FirstOrDefault();
                ViewBag.birthDate = String.Format("{0:MM/dd/yyyy}", userProfile.Birthdate);
                return View(userProfile);
            }

        }

        public ActionResult ProfileEdit(string id) {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                var currentUserId = User.Identity.GetUserId();
                if (Object.Equals(currentUserId, id))
                {
                    ViewBag.IsFanOrOwn = 0;
                }
                else
                {
                    var userFan = db.Fans_GetFanByUser(currentUserId, id).FirstOrDefault();

                    if (userFan != null)
                    {
                        if (userFan.SenderId == currentUserId)
                        {
                            ViewBag.FriendStatus = userFan.Status;
                            ViewBag.checkHaveAddFriend = 0;
                        }
                        if (userFan.RequestId == currentUserId)
                        {
                            ViewBag.FriendStatus = userFan.Status;
                            AspNetUsers_CheckAddFriend_Result checkHaveAddFriend = db.AspNetUsers_CheckAddFriend(id).FirstOrDefault();
                            if (checkHaveAddFriend != null)
                            {
                                ViewBag.checkHaveAddFriend = 1;
                            }
                            else
                            {
                                ViewBag.checkHaveAddFriend = 0;
                            }
                        }

                    }
                    else
                    {
                        ViewBag.checkHaveAddFriend = 0;
                        ViewBag.IsAddFan = 1;
                        ViewBag.FriendStatus = -1;
                    }


                    ViewBag.IsFanOrOwn = 1;
                }

                Users_GetById_Result userProfile = db.Users_GetById(id).FirstOrDefault();
                ViewBag.birthDate = String.Format("{0:MM/dd/yyyy}", userProfile.Birthdate);
                if(userProfile.Website!=null)
                {
                    string[] strs = userProfile.Website.Split('/');
                    userProfile.Website = strs.Last();
                    ViewBag.protocol = strs.First();
                }
               
                return View(userProfile);
            }
        }

        [HttpPost]
        public ActionResult ProfileEdit(Users_GetById_Result model, FormCollection collection)
        {
            
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                string protocol = collection["field12"];
                string website = protocol + model.Website;
                db.User_Update(model.Id, model.Name, model.Gender, model.Birthdate, model.Aboutme, website);
            }
            return RedirectToAction("UserProfile/"+model.Id);
        }

        

        [HttpPost]
        public JsonResult AddFriend(string id, string msg)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                var currentUserId = User.Identity.GetUserId();
                var result = db.Fans_Insert(currentUserId, id, 0, msg).FirstOrDefault();
                if (result > 0)
                {
                    return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    return new JsonResult { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }


            }

        }

        [HttpPost]
        public JsonResult ReplyFriend(string RequestId, string StatusReply)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                var currentUserId = User.Identity.GetUserId();
                int? result = db.Fans_Update(RequestId,currentUserId,StatusReply).FirstOrDefault();
                if (result > 0)
                {
                    return new JsonResult { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    return new JsonResult { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }

            }

        }
    }
}