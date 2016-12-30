using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFashion.Web
{
    public class FanHub : Hub
    {
        public static void ShowFan()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<FanHub>();
            context.Clients.All.fan("fan");
        }
    }
}