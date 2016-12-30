using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.SignalR;
using SocialFashion.Model.Models;
using System.Web.Mvc;
using SocialFashion.Web.Controllers;
using Microsoft.AspNet.Identity;

namespace SocialFashion.Hubs
{
    public class PostHub : Hub
    {
        //private const string AvatarPath = "/Images/Avatars/";
        // GET api/WallPost
        public void GetPosts()
        {
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                List<GetAllStatusById_Result> list = db.GetAllStatusById(currentUserId).ToList();
                var ret = (from post in list
                           orderby post.Date descending
                           select new
                           {
                               Message = post.Content,
                               PostedByName = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).Email,
                               PostedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).ImageAvartar,
                               PostedDate = post.Date,
                               PostId = post.StatusId,
                               CountLike = db.StatusLikes.Where(x => x.StatusId == post.StatusId && x.Type == 1).ToList().Count,
                               PostComments = from comment in db.StatusComments.ToList()
                                              where comment.StatusId == post.StatusId
                                              orderby comment.Date
                                              select new
                                              {
                                                  CommentedBy = comment.UserId,
                                                  CommentedByName = db.AspNetUsers.FirstOrDefault(x=>x.Id == comment.UserId).Email,
                                                  CommentedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == comment.UserId).ImageAvartar,
                                                  CommentedDate = comment.Date,
                                                  CommentId = comment.StatusCommentId,
                                                  Message = comment.Comment,
                                                  PostId = comment.StatusId
                                              }
                           }).ToArray();
                Clients.All.loadPosts(ret, currentUserId);
            }
        }

        public void GetPostProfile()
        {
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                List<Status> list = db.Status.Where(x=>x.UserId == currentUserId).ToList();
                var ret = (from post in list
                           orderby post.Date descending
                           select new
                           {
                               Message = post.Content,
                               PostedByName = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).Email,
                               PostedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).ImageAvartar,
                               PostedDate = post.Date,
                               PostId = post.StatusId,
                               CountLike = db.StatusLikes.Where(x => x.StatusId == post.StatusId && x.Type == 1).ToList().Count,
                               PostComments = from comment in db.StatusComments.ToList()
                                              where comment.StatusId == post.StatusId
                                              orderby comment.Date
                                              select new
                                              {
                                                  CommentedBy = comment.UserId,
                                                  CommentedByName = db.AspNetUsers.FirstOrDefault(x => x.Id == comment.UserId).Email,
                                                  CommentedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == comment.UserId).ImageAvartar,
                                                  CommentedDate = comment.Date,
                                                  CommentId = comment.StatusCommentId,
                                                  Message = comment.Comment,
                                                  PostId = comment.StatusId
                                              }
                           }).ToArray();
                Clients.All.loadPosts(ret, currentUserId);
            }
        }

        public void AddPost(Status post)
        {
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            Controller c = new StatusController();
            post.Date = DateTime.UtcNow;
            post.UserId = currentUserId;
            post.MoreImages = "Img";
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                db.Status.Add(post);
                db.SaveChanges();
                var ret = new
                {
                    Message = post.Content,
                    PostedBy = post.UserId,
                    PostedByName = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).Email,
                    PostedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == post.UserId).ImageAvartar,
                    PostedDate = post.Date,
                    PostId = post.StatusId
                };
                Clients.Caller.addPost(ret);
                Clients.Others.newPost(ret, post.Privacy, post.UserId);
            }
        }

        public dynamic AddComment(StatusComment postcomment)
        {
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            postcomment.Date = DateTime.UtcNow;
            postcomment.UserId = currentUserId;
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                db.StatusComments.Add(postcomment);
                db.SaveChanges();
                var ret = new
                {
                    CommentedBy = postcomment.UserId,
                    CommentedByName = db.AspNetUsers.FirstOrDefault(x => x.Id == postcomment.UserId).Email,
                    CommentedByAvatar = db.AspNetUsers.FirstOrDefault(x => x.Id == postcomment.UserId).ImageAvartar,
                    CommentedDate = postcomment.Date,
                    CommentId = postcomment.StatusCommentId,
                    Message = postcomment.Comment,
                    PostId = postcomment.StatusId
                };
                Clients.Others.newComment(ret, postcomment.StatusId);
                return ret;
            }
        }

        public dynamic AddLike(StatusLike like)
        {
            int nCount = 0;
            var currentUserId = string.Empty;
            if (System.Web.HttpContext.Current != null)
            {
                currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            }
            like.UserId = currentUserId;
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                StatusLike sttLike = db.StatusLikes.FirstOrDefault(l => l.StatusId == like.StatusId && l.UserId == like.UserId);
                if(sttLike == null)
                {
                    like.Type = 1;
                    db.StatusLikes.Add(like);
                    db.SaveChanges();
                    nCount = db.StatusLikes.Where(x => x.StatusId == like.StatusId && x.Type == 1).ToList().Count;
                }
                else
                {
                    if (sttLike.Type == 1)
                    {
                        sttLike.Type = 0;
                    }
                    else
                    {
                        sttLike.Type = 1;
                    }
                    db.SaveChanges();
                    nCount = db.StatusLikes.Where(x => x.StatusId == sttLike.StatusId && x.Type == 1).ToList().Count;
                }
                Clients.Others.newLike(nCount, like.StatusId);
            }
            return nCount;
        }

        public dynamic IsFriend(String user1, String user2)
        {
            using (SocialFashionDbContext db = new SocialFashionDbContext())
            {
                return db.Fans.Where(x => (x.RequestId == user1 && x.SenderId == user2) || (x.RequestId == user2 && x.SenderId == user1)).ToList().Count;
            }
        }
    }
}