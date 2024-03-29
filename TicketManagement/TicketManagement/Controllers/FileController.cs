﻿using System.Threading.Tasks;
using System.Web.Mvc;
using TicketManagement.Management;
using TicketManagement.Models.Context;

namespace TicketManagement.Controllers
{
    [Authorize(Roles = MyRoles.Approved)]
    public class FileController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        public async Task<ActionResult> Index(int id)
        {
            var fileToRetrieve = await db.Files.FindAsync(id);
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}
