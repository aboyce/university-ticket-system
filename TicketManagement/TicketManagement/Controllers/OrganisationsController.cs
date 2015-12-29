﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketManagement.Models.Context;
using TicketManagement.Models.Entities;
using TicketManagement.Models.Management;

namespace TicketManagement.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class OrganisationsController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        public async Task<ActionResult> Index()
        {
            var organisations = db.Organisations.Include(o => o.DefaultContact);
            return View(await organisations.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Organisation organisation = await db.Organisations.FindAsync(id);

            if (organisation == null)
                return HttpNotFound();
            
            return View(organisation);
        }

        public ActionResult Create()
        {
            ViewBag.PossibleDefaultContacts = new SelectList(db.Users, "Id", "FullName");
            return View();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,IsInternal,DefaultContactId,LastUpdated")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                string defaulContactId = Request.Form["PossibleDefaultContacts"];

                organisation.DefaultContactId = defaulContactId;
                organisation.DefaultContact = await db.Users.FirstOrDefaultAsync(u => u.Id == defaulContactId);

                db.Organisations.Add(organisation);
                await db.SaveChangesAsync();

                return RedirectToAction("Index", new { ViewMessage = ViewMessage.OrganisationUpdated});
            }

            ViewBag.PossibleDefaultContacts = new SelectList(db.Users, "Id", "FullName", organisation.DefaultContactId);
            return View(organisation);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Organisation organisation = await db.Organisations.FindAsync(id);

            if (organisation == null)
                return HttpNotFound();
            
            ViewBag.PossibleDefaultContacts = new SelectList(db.Users, "Id", "FullName", organisation.DefaultContactId);

            return View(organisation);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,IsInternal,DefaultContactId,LastUpdated")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                string defaulContactId = Request.Form["PossibleDefaultContacts"];

                organisation.DefaultContactId = defaulContactId;
                organisation.DefaultContact = await db.Users.FirstOrDefaultAsync(u => u.Id == defaulContactId);
                organisation.LastUpdated = DateTime.Now;

                db.Entry(organisation).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index", new { ViewMessage = ViewMessage.OrganisationUpdated });
            }

            ViewBag.PossibleDefaultContacts = new SelectList(db.Users, "Id", "FullName", organisation.DefaultContactId);

            return View(organisation);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Organisation organisation = await db.Organisations.FindAsync(id);

            if (organisation == null)
                return HttpNotFound();
            
            return View(organisation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Organisation organisation = await db.Organisations.FindAsync(id);

            db.Organisations.Remove(organisation);
            await db.SaveChangesAsync();

            return RedirectToAction("Index", new { ViewMessage = ViewMessage.OrganisationDeleted });
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
