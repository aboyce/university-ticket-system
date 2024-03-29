﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace TicketManagement.ViewModels
{
    internal class UserViewModels
    {
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public bool TwitterEnabled { get; set; } = false;
        public bool TwitterConfigured { get; set; } = false;

        public UserLoginInfo FacebookConfigured { get; set; } = null;
        public AuthenticationDescription FacebookToConfigure { get; set; } = null;
    }
}
