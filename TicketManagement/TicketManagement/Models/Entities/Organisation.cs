﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManagement.Models.Entities
{
    public class Organisation : EntityBase
    {
        private string _name;
        private bool _isInternal = false;
        private int? _contactUserId = null;
        private UserExtra _defaultContact = null;

        [Required]
        [StringLength(50, ErrorMessage = "Name must be less that 50 characters but more than 2", MinimumLength = 2)]
        public string Name
        {
            get { return _name; }
            set { _name = value; Updated(); }
        }

        [Required]
        [DisplayName("Is Internal")]
        public bool IsInternal
        {
            get { return _isInternal; }
            set { _isInternal = value; Updated(); }
        }

        [ForeignKey("DefaultContact")]
        [DisplayName("Default Contact")]
        public int? ContactUserId
        {
            get { return _contactUserId; }
            set { _contactUserId = value; Updated(); }
        }

        public virtual UserExtra DefaultContact
        {
            get { return _defaultContact; }
            set { _defaultContact = value; Updated(); }
        }
    }
}
