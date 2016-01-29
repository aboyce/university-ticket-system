﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Facebook;
using TicketManagement.Helpers;
using TicketManagement.Management;
using TicketManagement.ViewModels;

namespace TicketManagement.Controllers
{
    [Authorize(Roles = MyRoles.Social)]
    public class FacebookController : FacebookErrorHandlerController
    {
        public ActionResult Index()
        {
            if (!HttpContext.Items.Contains("access_token"))
                return View(new FacebookIndexViewModel { IsLoggedIn = false });

            string accessToken = HttpContext.Items["access_token"].ToString();

            return View(string.IsNullOrEmpty(accessToken) ? new FacebookIndexViewModel { IsLoggedIn = false } : new FacebookIndexViewModel { IsLoggedIn = true });
        }

        public async Task<ActionResult> _Partial_FacebookProfileSummary()
        {
            string accessToken = GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
                return FacebookError("");

            FacebookClient fb = new FacebookClient(accessToken);

            dynamic userInfo = await fb.GetTaskAsync("me?fields=first_name,last_name,email,locale,birthday,link,location,gender");

            return PartialView(FacebookHelpers.ToStatic<FacebookProfileSummaryViewModel>(userInfo));
        }

        public async Task<ActionResult> _Partial_FacebookPage()
        {
            string accessToken = GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
                return FacebookError("");

            FacebookClient fb = new FacebookClient(accessToken);

            dynamic page = await fb.GetTaskAsync($"{await ConfigurationHelper.GetFacebookPageIdAsync()}?fields=id, name, business, link, can_post, is_published, category, description, likes, new_like_count, unread_message_count, unseen_message_count, unread_notif_count, talking_about_count");

            FacebookPageViewModel vm = FacebookHelpers.ToStatic<FacebookPageViewModel>(page);

            return PartialView(vm.Id != null ? vm : null);
        }

        public async Task<ActionResult> _Partial_FacebookPagePosts()
        {
            string facebookCommand =
                $"{await ConfigurationHelper.GetFacebookPageIdAsync()}/posts?limit=3&fields=id,message,story,name,link,picture,place,story,likes,description,is_hidden,is_published,caption,created_time,updated_time,shares";

            FacebookPagePosts pagePosts = await HandleFacebookPagePostsAsync(facebookCommand);

            if (pagePosts == null)
                return FacebookError("Problem contacting Facebook, could be a problem with your access token, connection, or request type.");

            ViewBag.ShowGetMorePagePosts = pagePosts.ShowGetMorePagePosts;

            return PartialView(pagePosts.PageList);
        }

        [HttpPost]
        public async Task<ActionResult> GetMorePagePosts(string nextPageUri)
        {
            if (string.IsNullOrEmpty(nextPageUri)) return null;

            FacebookPagePosts pagePosts = await HandleFacebookPagePostsAsync(nextPageUri);

            if(pagePosts == null)
                return FacebookError("Problem contacting Facebook, could be a problem with your access token, connection, or request type.");

            ViewBag.ShowGetMorePagePosts = pagePosts.ShowGetMorePagePosts;

            return PartialView("_Partial_FacebookPagePosts", pagePosts.PageList);
        }

        private async Task<FacebookPagePosts> HandleFacebookPagePostsAsync(string facebookCommand)
        {
            FacebookPagePosts toReturn = new FacebookPagePosts { PageList = new List<FacebookPagePostViewModel>() };

            string accessToken = GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
                return null;

            FacebookClient fb = new FacebookClient(accessToken);

            dynamic pagePosts = await fb.GetTaskAsync(facebookCommand);

            foreach (dynamic post in pagePosts)
            {
                FacebookPagePostViewModel currentPost = FacebookHelpers.ToStatic<FacebookPagePostViewModel>(post);

                if (currentPost == null) continue;

                if (post.place != null)
                {
                    currentPost.Place = FacebookHelpers.ToStatic<FacebookPlaceViewModel>(post.place);
                    currentPost.Place.Location = post.place.location != null ? FacebookHelpers.ToStatic<FacebookLocationViewModel>(post.place.location) : null;
                }

                toReturn.PageList.Add(currentPost);
            }

            if (pagePosts.paging == null || pagePosts.paging.next == null) return toReturn;

            toReturn.ShowGetMorePagePosts = pagePosts.paging.next;
            toReturn.ShowGetMorePagePosts = toReturn.ShowGetMorePagePosts.Replace($"https://graph.facebook.com/v{ConfigurationHelper.GetFacebookGraphApiVersion()}/", "").Replace($"&access_token={accessToken}", "");

            return toReturn;
        }

        private class FacebookPagePosts
        {
            public List<FacebookPagePostViewModel> PageList { get; set; }

            public string ShowGetMorePagePosts { get; set; }
        }

        #region Testing

        public async Task<ActionResult> Test_RevokeAccessToken()
        {
            var accessToken = GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
                throw new HttpException(404, "Missing Access Token");

            FacebookClient fb = new FacebookClient(accessToken);

            dynamic userFeed = await fb.DeleteTaskAsync("me/permissions");
            return RedirectToAction("Index", "Tickets");
        }

        public Action Test_ApiLimit()
        {
            throw new FacebookApiLimitException();
        }

        #endregion
    }
}