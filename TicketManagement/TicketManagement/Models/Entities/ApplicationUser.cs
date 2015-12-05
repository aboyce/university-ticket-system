﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TicketManagement.Models.Entities
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public ApplicationUser() {}
        public ApplicationUser(string email, string firstName, string lastName, string userName, bool isArchived = false)
        {
            try
            {
                UserExtra = new UserExtra { FirstName = firstName, LastName = lastName, IsArchived = isArchived, ApplicationUserId = Id };
                UserExtraId = UserExtra.Id;
                UserName = userName;
                Email = email;
            }
            catch (Exception)
            {
                UserExtra = new UserExtra { FirstName = "First", LastName = "Last", IsArchived = false, ApplicationUserId = Id};
            }

        }

        [ForeignKey("UserExtra")]
        [DisplayName("User")]
        public int? UserExtraId { get; set; } = null;

        public virtual UserExtra UserExtra { get; set; } = null;
    }
}
