using SocialFashion.Common;
using SocialFashion.Model.Models;
using SocialFashion.Service;
using SocialFashion.Web.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialFashion.Web.Controllers
{
    [Authorize(Roles = "user")]
    public class MembersController : Controller
    {
        IAspNetUserService _aspNetUserService;
        public MembersController(IAspNetUserService aspNetUserService)
        {
            this._aspNetUserService = aspNetUserService;
        }
        // GET: Members
        public ActionResult Index(int page = 1, string sort = "")
        {
            int pageSize = int.Parse(ConfigHelper.GetByKey("PageSize"));
            
            int totalRow = 0;
            
            var aspNetUserModel = _aspNetUserService.GetListAspNetUserByIdPaging(page, pageSize, sort, out totalRow);
            
            int totalPage = (int)Math.Ceiling((double)totalRow / pageSize);

           
            var paginationSet = new PaginationSet<AspNetUser>()
            {
                Items = aspNetUserModel,
                MaxPage = int.Parse(ConfigHelper.GetByKey("MaxPage")),
                Page = page,
                TotalCount = totalRow,
                TotalPages = totalPage
            };

            return View(paginationSet);
        }

    }
}