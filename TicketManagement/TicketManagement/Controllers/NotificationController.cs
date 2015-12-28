﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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

        public async Task<ActionResult> AutoriseNotification(NotificationViewModel vm, NotificationCategory notificationCategory, int notificationId)
        {
            bool success = false;

            if (notificationId > 0)
            {
                if (notificationCategory == NotificationCategory.User)
                {
                    success = await Helpers.NotificationHelper.UndertakeNotification(db, UserManager,
                        un: await db.UserNotifications.Include(un => un.NotificationAbout).FirstOrDefaultAsync(un => un.Id == notificationId));
                }
                else if (notificationCategory == NotificationCategory.Role)
                {
                    success = await Helpers.NotificationHelper.UndertakeNotification(db, UserManager,
                        rn: await db.RoleNotifications.Include(rn => rn.NotificationAbout).Include(rn => rn.Role).FirstOrDefaultAsync(rn => rn.Id == notificationId));
                }
            }

            return RedirectToAction("Index", success ? new { ViewMessage = ViewMessage.AppliedRoleFromNotification } : new { ViewMessage = ViewMessage.FailedToApplyRoleFromNotification });
        }

        public async Task<ActionResult> DeclineNotification(NotificationViewModel vm, NotificationCategory notificationCategory, int notificationId)
        {
            bool success = false;

            if (notificationId > 0)
            {
                if (notificationCategory == NotificationCategory.User)
                {
                    success = await Helpers.NotificationHelper.DeclineNotification(db, UserManager,
                        un: await db.UserNotifications.Include(un => un.NotificationAbout).FirstOrDefaultAsync(un => un.Id == notificationId)); 
                }
                else if (notificationCategory == NotificationCategory.Role)
                {
                    success =
                        await
                            Helpers.NotificationHelper.DeclineNotification(db, UserManager,
                                rn: await db.RoleNotifications.Include(rn => rn.NotificationAbout).Include(rn => rn.Role).FirstOrDefaultAsync(rn => rn.Id == notificationId));
                }
            }

            return RedirectToAction("Index", success ? new { ViewMessage = ViewMessage.DeclinedRoleFromNotification } : new { ViewMessage = ViewMessage.FailedToDeclineRoleFromNotification });
        }

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
