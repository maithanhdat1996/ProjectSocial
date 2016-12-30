using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SocialFashion.Model.Models;

namespace SocialFashion.Web.Controllers
{
    [Authorize(Roles = "user")]
    public class AvatarController : Controller
    {
        private const int AvatarStoredWidth = 100;  // ToDo - Change the size of the stored avatar image
        private const int AvatarStoredHeight = 100; // ToDo - Change the size of the stored avatar image
        private const int AvatarScreenWidth = 400;  // ToDo - Change the value of the width of the image on the screen

        private const string TempFolder = "/Temp";
        private const string MapTempFolder = "~" + TempFolder;
        private const string AvatarPath = "/Images/Avatars";
        private const string CoverPath = "/Images/Covers";

        private readonly string[] _imageFileExtensions = { ".jpg", ".png", ".gif", ".jpeg" };

        
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveCover(string position, string imageFile)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                var currentId = User.Identity.GetUserId();
                var user =  db.AspNetUsers.Where(x => x.Id == currentId).SingleOrDefault();
                if (user != null)
                {
                    var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(imageFile));
                    var img = new WebImage(fn);
                    
                    // ... delete the temporary file,...
                    System.IO.File.Delete(fn);
                    // ... and save the new one.
                    var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                    var newFileLocation = HttpContext.Server.MapPath(newFileName);
                    if (Directory.Exists(Path.GetDirectoryName(newFileLocation)) == false)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                    }

                    img.Save(newFileLocation);

                    user.ImageCover = newFileName;
                    user.Position_Cover = position;
                    db.SaveChanges();
                    return RedirectToAction("UserProfile", "User", new { id = User.Identity.GetUserId() });
                }
                else {
                    return View();
                        }
            }

                
        }

        [ValidateAntiForgeryToken]
        public ActionResult _Upload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) return Json(new { success = false, errorMessage = "No file uploaded." });
            var file = files.FirstOrDefault();  // get ONE only
            if (file == null || !IsImage(file)) return Json(new { success = false, errorMessage = "File is of wrong format." });
            if (file.ContentLength <= 0) return Json(new { success = false, errorMessage = "File cannot be zero length." });
            var webPath = GetTempSavedFilePath(file);
            //mistertommat - 18 Nov '15 - replacing '\' to '//' results in incorrect image url on firefox and IE,
            //                            therefore replacing '\\' to '/' so that a proper web url is returned.            
            return Json(new { success = true, fileName = webPath.Replace("\\", "/") }); // success
        }

        public ActionResult _UploadCover(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) return Json(new { success = false, errorMessage = "No file uploaded." });
            var file = files.FirstOrDefault();  // get ONE only
            if (file == null || !IsImage(file)) return Json(new { success = false, errorMessage = "File is of wrong format." });
            if (file.ContentLength <= 0) return Json(new { success = false, errorMessage = "File cannot be zero length." });
            var webPath = GetTempSavedFilePath(file);
            //mistertommat - 18 Nov '15 - replacing '\' to '//' results in incorrect image url on firefox and IE,
            //                            therefore replacing '\\' to '/' so that a proper web url is returned.            
            return Json(new { success = true, fileName = webPath.Replace("\\", "/") }); // success
        }


        [HttpPost]
        public ActionResult Save(string t, string l, string h, string w, string fileName)
        {
            try
            {
                // Calculate dimensions
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                // Get file from temporary folder
                var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName));
                // ...get image and resize it, ...
                var img = new WebImage(fn);
                img.Resize(width, height);
                // ... crop the part the user selected, ...
                img.Crop(top, left, img.Height - top - AvatarStoredHeight, img.Width - left - AvatarStoredWidth);
                // ... delete the temporary file,...
                System.IO.File.Delete(fn);
                // ... and save the new one.
                var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                var newFileLocation = HttpContext.Server.MapPath(newFileName);
                if (Directory.Exists(Path.GetDirectoryName(newFileLocation)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                }

                img.Save(newFileLocation);
                var currentUserId = User.Identity.GetUserId();
                using (SocialFashionDbContext db = new SocialFashionDbContext())
                {
                    db.AspNetUsers.SingleOrDefault(m => m.Id == currentUserId).ImageAvartar = newFileName;
                    db.SaveChanges();
                }
                return Json(new { success = true, avatarFileLocation = newFileName,id = currentUserId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Unable to upload file.\nERRORINFO: " + ex.Message });
            }
        }

        

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image") ||
                _imageFileExtensions.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTempSavedFilePath(HttpPostedFileBase file)
        {
            // Define destination
            var serverPath = HttpContext.Server.MapPath(TempFolder);
            if (Directory.Exists(serverPath) == false)
            {
                Directory.CreateDirectory(serverPath);
            }

            // Generate unique file name
            var fileName = Path.GetFileName(file.FileName);
            fileName = SaveTemporaryAvatarFileImage(file, serverPath, fileName);

            // Clean up old files after every save
            CleanUpTempFolder(1);
            return Path.Combine(TempFolder, fileName);
        }

        private static string SaveTemporaryAvatarFileImage(HttpPostedFileBase file, string serverPath, string fileName)
        {
            var img = new WebImage(file.InputStream);
            var ratio = img.Height / (double)img.Width;
            img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));

            var fullFileName = Path.Combine(serverPath, fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }

            img.Save(fullFileName);
            return Path.GetFileName(img.FileName);
        }

        private void CleanUpTempFolder(int hoursOld)
        {
            try
            {
                var currentUtcNow = DateTime.UtcNow;
                var serverPath = HttpContext.Server.MapPath("/Temp");
                if (!Directory.Exists(serverPath)) return;
                var fileEntries = Directory.GetFiles(serverPath);
                foreach (var fileEntry in fileEntries)
                {
                    var fileCreationTime = System.IO.File.GetCreationTimeUtc(fileEntry);
                    var res = currentUtcNow - fileCreationTime;
                    if (res.TotalHours > hoursOld)
                    {
                        System.IO.File.Delete(fileEntry);
                    }
                }
            }
            catch
            {
                // Deliberately empty.
            }
        }
    }
}