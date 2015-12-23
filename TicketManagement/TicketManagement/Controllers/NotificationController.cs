﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TicketManagement.Models.Context;
using TicketManagement.Models.Entities;
using TicketManagement.Models.Management;
using TicketManagement.ViewModels;

namespace TicketManagement.Controllers
{
    public class NotificationController : Controller
    {
        private ApplicationContext db = new ApplicationContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //
        //// GET: Index
        public ActionResult Index(NotificationViewModel vm)
        {
            User user = UserManager.FindById(User.Identity.GetUserId());

            vm.RoleNotifications = Helpers.NotificationHelper.GetRoleNotificationsForUser(db, user.Id, UserManager.GetRoles(user.Id));
            vm.UserNotifications = Helpers.NotificationHelper.GetUserNotificationsForUser(db, user.Id);

            return View(vm);
        }

        [ChildActionOnly]
        public ActionResult _Partial_Notifications(NotificationViewModel vm)
        {
            User user = UserManager.FindById(User.Identity.GetUserId());

            vm.RoleNotifications = Helpers.NotificationHelper.GetRoleNotificationsForUser(db, user.Id, UserManager.GetRoles(user.Id));
            vm.UserNotifications = Helpers.NotificationHelper.GetUserNotificationsForUser(db, user.Id);

            return PartialView(vm);
        }

        public ActionResult AutoriseNotification(NotificationViewModel vm, NotificationCategory notificationCategory, int notificationId)
        {
            bool success = false;

            if (notificationId > 0)
            {
                if (notificationCategory == NotificationCategory.User)
                {
                    success = Helpers.NotificationHelper.UndertakeNotification(db, UserManager, un: db.UserNotifications.FirstOrDefault(un => un.Id == notificationId));
                }
                else if (notificationCategory == NotificationCategory.Role)
                {
                    success = Helpers.NotificationHelper.UndertakeNotification(db, UserManager, rn: db.RoleNotifications.FirstOrDefault(rn => rn.Id == notificationId));
                }
            }

            return RedirectToAction("Index", success ? new { ViewMessage = ViewMessage.AppliedRoleFromNotification } : new { ViewMessage = ViewMessage.FailedToApplyRoleFromNotification });
        }

        [HttpPost]
        public ActionResult Test()
        {
            return View("Index");
        }

        //// GET: Notification/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Notification/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,UserIdNotificationOn,NotificationType,NotificationMessage")] UserNotification userNotification)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.UserNotifications.Add(userNotification);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(userNotification);
        //}

        //// POST: Notification/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    UserNotification userNotification = await db.UserNotifications.FindAsync(id);
        //    db.UserNotifications.Remove(userNotification);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}