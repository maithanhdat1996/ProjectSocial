using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFashion.Web.Models
{
    public class UserDetail
    {
        public string ConnectionId { get; set; }
        public string  Email { get; set; }
        public string UserName { get; set; }
        public string ImageAvatar { get; set; }
    }
}