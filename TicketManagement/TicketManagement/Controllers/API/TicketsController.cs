﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketManagement.Helpers;
using TicketManagement.Models.Context;
using TicketManagement.Models.Entities;

namespace TicketManagement.Controllers.API
{
    [AllowAnonymous]
    public class TicketsController : BaseApiController
    {
        public TicketsController() {}
        public TicketsController(IApplicationContext context)
        { db = context; }

        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<JsonResult> GetAllTicketsForUser(string username, string usertoken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(usertoken)) return null;

            // Try to get the specific user with the Username and UserToken
            string userId =
                await
                    db.Users.Where(u => u.UserName == username && u.UserToken == usertoken)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(userId)) return null;

            // Build a query for all the tickets and linked information. (Not an actual DB request)
            var tickets =
                db.Tickets.Include(t => t.OpenedBy)
                    .Include(t => t.OrganisationAssignedTo)
                    .Include(t => t.Project)
                    .Include(t => t.TeamAssignedTo)
                    .Include(t => t.TicketCategory)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketState);

            if (!await IsUserInternal(userId))
                tickets = tickets.Where(t => t.OpenedById == userId);

            return new JsonResult
            {
                ContentType = "Tickets",
                Data = tickets.Select(Helpers.ApiHelper.GetApiTicketViewModel).ToList()
            };
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<JsonResult> GetTicketForUser(string ticketid, string username, string usertoken)
        {
            if (string.IsNullOrEmpty(ticketid) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(usertoken))
                return null;

            // See if the Ticket Id is a valid value, and cast to an int.
            int id;
            if (!int.TryParse(ticketid, out id)) return null;

            // Try to get the specific user with the Username and UserToken
            string userId =
                await
                    db.Users.Where(u => u.UserName == username && u.UserToken == usertoken)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(userId)) return null;

            // Build a query for the ticket and linked information. (Not an actual DB request)
            var ticketQuery =
                db.Tickets.Include(t => t.OpenedBy)
                    .Include(t => t.OrganisationAssignedTo)
                    .Include(t => t.Project)
                    .Include(t => t.TeamAssignedTo)
                    .Include(t => t.TicketCategory)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketState);

            // Try to get the ticket
            Ticket ticket = await ticketQuery.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return null;

            // If the user is internal, then we can give them the ticket information.
            if (await IsUserInternal(userId))
            {
                return new JsonResult
                {
                    ContentType = "Ticket",
                    Data = Helpers.ApiHelper.GetApiTicketViewModel(ticket)
                };
            }

            // If the user is not internal, we have to check that they opened the ticket, aka it is one of theirs before giving them the information.
            else
            {
                if (ticket.OpenedById != userId)
                    return null;

                return new JsonResult
                {
                    ContentType = "Ticket",
                    Data = Helpers.ApiHelper.GetApiTicketViewModel(ticket)
                };
            }
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<JsonResult> GetTicketLogsForUser(string ticketid, string username, string usertoken)
        {
            if (string.IsNullOrEmpty(ticketid) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(usertoken))
                return null;

            // See if the Ticket Id is a valid value, and cast to an int.
            int id;
            if (!int.TryParse(ticketid, out id)) return null;

            // Build a query for the ticket and linked information. (Not an actual DB request)
            var ticketQuery =
                db.Tickets.Include(t => t.OpenedBy)
                    .Include(t => t.OrganisationAssignedTo)
                    .Include(t => t.Project)
                    .Include(t => t.TeamAssignedTo)
                    .Include(t => t.TicketCategory)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketState);

            // Try to get the ticket
            Ticket ticket = await ticketQuery.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return null;

            // Try to get the specific user with the Username and UserToken
            string userId =
                await
                    db.Users.Where(u => u.UserName == username && u.UserToken == usertoken)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(userId)) return null;

            // Get a list of the Ticket Logs that are related to the ticket
            var ticketLogs =
                db.TicketLogs.Where(tl => tl.TicketId == id).Include(tl => tl.Ticket).Include(tl => tl.SubmittedByUser);

            // If the user is not internal...
            if (!await IsUserInternal(userId))
            {
                // ... they must have opened it to be able to access the information, else they don' have permission to view the information.
                if (ticket.OpenedById != userId)
                    return null;

                // ... exclude any internal messages regarding the ticket, as they also don't have permission to view the information
                ticketLogs = ticketLogs.Where(tl => tl.IsInternal == false);
            }

            return new JsonResult
            {
                ContentType = "TicketLogs",
                Data = ticketLogs.Select(Helpers.ApiHelper.GetApiTicketLogViewModel).ToList()
            };
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<bool> AddExternalReplyToTicket(string ticketid, string username, string usertoken, string message)
        {
            return await AddReplyToTicket(ticketid, username, usertoken, message, false);
        }

        [System.Web.Http.AcceptVerbs("GET")]
        public async Task<bool> AddInternalReplyToTicket(string ticketid, string username, string usertoken, string message)
        {
            return await AddReplyToTicket(ticketid, username, usertoken, message, true);
        }

        private async Task<bool> AddReplyToTicket(string ticketid, string username, string usertoken, string message, bool isInternal = false)
        {
            if (string.IsNullOrEmpty(ticketid) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(usertoken) ||
                string.IsNullOrEmpty(message)) return false;

            // See if the Ticket Id is a valid value, and cast to an int.
            int id;
            if (!int.TryParse(ticketid, out id)) return false;

            // Try to get the ticket
            Ticket ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return false;

            // Try to get the specific user with the Username and UserToken
            string userId =
                await
                    db.Users.Where(u => u.UserName == username && u.UserToken == usertoken)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(userId)) return false;

            TicketLogType type = await IsUserInternal(userId)
                ? TicketLogType.MessageFromInternalUser
                : TicketLogType.MessageFromExternalUser;

            // If the user is not internal they must have opened it to be able to access it, else they don't have permission to add to the ticket.
            if (!await IsUserInternal(userId) && ticket.OpenedById != userId)
                return false;

            return await TicketLogHelper.NewTicketLogAsync(userId, id, type, isInternal, false, db, message, null);
        }
    }
}
