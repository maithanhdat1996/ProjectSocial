using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFashion.Web
{
    public class MessageHub : Hub
    {
        public static void ShowMsg()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            context.Clients.All.msg("msg");
        }
    }
}