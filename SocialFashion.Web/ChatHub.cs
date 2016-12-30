using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SocialFashion.Model.Models;
using SocialFashion.Web.Models;

namespace SocialFashion.Web
{
    public class ChatHub : Hub
    {
        public static string emailIDLoaded = "";
         List<UserDetail> lstUserConnected = new List<UserDetail>();
        #region Connect
        public void Connect(string userName, string email)
        {
            emailIDLoaded = email;
            var id = Context.ConnectionId;
            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {
                var item = dc.AspNetUsers.FirstOrDefault(x => x.Email == email && x.IsOnline == true);
                if (item != null)
                {
                    item.IsOnline = false;
                    dc.SaveChanges();

                    // Disconnect
                    Clients.All.onUserDisconnectedExisting(item.ConnectionId, item.UserName);
                }


                var Users = dc.AspNetUsers.ToList();
                if (Users.Where(x => x.Email == email && x.IsOnline == true).ToList().Count == 0)
                {
                    var item1 = dc.AspNetUsers.FirstOrDefault(x => x.Email == email && x.IsOnline == false);
                    item1.ConnectionId = id;
                    item1.IsOnline = true;
                    dc.SaveChanges();

                    var connectedUsers = dc.AspNetUsers.Where(x => x.IsOnline == true && x.Email != email).ToList();
                    foreach (var userConnect in connectedUsers)
                    {
                        UserDetail u = new UserDetail();
                        u.ConnectionId = userConnect.ConnectionId;
                        u.Email = userConnect.Email;
                        u.UserName = userConnect.Name;
                        u.ImageAvatar = userConnect.ImageAvartar;
                        lstUserConnected.Add(u);
                    }
                    Clients.All.onUserDisconnectedExisting(item1.ConnectionId, item1.UserName);
                    Clients.Caller.onConnected(id, userName, lstUserConnected, "");
                }
                var newUserConnected = dc.AspNetUsers.FirstOrDefault(x => x.Email == email && x.IsOnline == true);
                var nameUser = newUserConnected.Name;
                var imageAvatar = newUserConnected.ImageAvartar;
                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, nameUser, email, imageAvatar);
            }
        }
        #endregion

        #region Disconnect
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {
                var item = dc.AspNetUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (item != null)
                {
                    item.IsOnline = false;
                    dc.SaveChanges();
                    var id = Context.ConnectionId;
                    Clients.All.onUserDisconnected(id, item.UserName);

                }
            }
            return base.OnDisconnected(stopCalled);
        }
        #endregion

        #region Send_To_All
        //public void SendMessageToAll(string userName, string message)
        //{
        //    // store last 100 messages in cache
        //    AddAllMessageinCache(userName, message);

        //    // Broad cast message
        //    Clients.All.messageReceived(userName, message);
        //}
        #endregion

        #region Private_Messages
        public void SendPrivateMessage(string toUserId, string message, string status)

        {
            string fromUserId = Context.ConnectionId;
            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {
                var toUser = dc.AspNetUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
                var fromUser = dc.AspNetUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);
                if (toUser != null && fromUser != null)
                {
                    if (status == "Click")
                        AddPrivateMessageinCache(fromUser.Email, toUser.Email, fromUser.UserName, message);

                    // send to 
                    Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message, fromUser.Email, toUser.Email, status, fromUserId);

                    // send to caller user
                    Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message, fromUser.Email, toUser.Email, status, fromUserId);
                }
            }
        }
        public List<PrivateChatMessage> GetPrivateMessage(string fromid, string toid, int take)
        {
            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {
                List<PrivateChatMessage> msg = new List<PrivateChatMessage>();

                var v = (from a in dc.AspNetUsers
                         join b in dc.Messages on a.Email equals b.SenderEmail into cc
                         from c in cc
                         where (c.SenderEmail.Equals(fromid) && c.RecieverEmail.Equals(toid)) || (c.SenderEmail.Equals(toid) && c.RecieverEmail.Equals(fromid))
                         orderby c.MessageId descending
                         select new
                         {
                             UserName = a.UserName,
                             Message = c.Content,
                             ID = c.MessageId
                         }).Take(take).ToList();
                v = v.OrderBy(s => s.ID).ToList();

                foreach (var a in v)
                {
                    var res = new PrivateChatMessage()
                    {
                        userName = a.UserName,
                        message = a.Message
                    };
                    msg.Add(res);
                }
                return msg;
            }
        }

        private int takeCounter = 0;
        private int skipCounter = 0;
        public List<PrivateChatMessage> GetScrollingChatData(string fromid, string toid, int start = 10, int length = 1)
        {
            takeCounter = (length * start); // 20
            skipCounter = ((length - 1) * start); // 10

            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {
                List<PrivateChatMessage> msg = new List<PrivateChatMessage>();
                var v = (from a in dc.AspNetUsers
                         join b in dc.Messages on a.Email equals b.SenderEmail into cc
                         from c in cc
                         where (c.SenderEmail.Equals(fromid) && c.RecieverEmail.Equals(toid)) || (c.SenderEmail.Equals(toid) && c.RecieverEmail.Equals(fromid))
                         orderby c.MessageId descending
                         select new
                         {
                             UserName = a.UserName,
                             Message = c.Content,
                             ID = c.MessageId
                         }).Take(takeCounter).Skip(skipCounter).ToList();

                foreach (var a in v)
                {
                    var res = new PrivateChatMessage()
                    {
                        userName = a.UserName,
                        message = a.Message
                    };
                    msg.Add(res);
                }
                return msg;
            }
        }
        #endregion

        #region Save_Cache
        //private void AddAllMessageinCache(string userName, string message)
        //{
        //    using (Surajit_TestEntities dc = new Surajit_TestEntities())
        //    {
        //        var messageDetail = new ChatMessageDetail
        //        {
        //            UserName = userName,
        //            Message = message,
        //            EmailID = emailIDLoaded
        //        };
        //        dc.ChatMessageDetails.Add(messageDetail);
        //        dc.SaveChanges();
        //    }
        //}

        private void AddPrivateMessageinCache(string fromEmail, string chatToEmail, string userName, string message)
        {
            using (SocialFashionDbContext dc = new SocialFashionDbContext())
            {

                var resultDetails = new Message
                {
                    SenderEmail = fromEmail,
                    RecieverEmail = chatToEmail,
                    Content = message,
                    Date = DateTime.Now,
                    IsRead = false
                };
                dc.Messages.Add(resultDetails);
                dc.SaveChanges();
            }
        }
        #endregion
    }

    public class PrivateChatMessage
    {
        public string userName { get; set; }
        public string message { get; set; }
    }
}