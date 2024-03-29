﻿using System.Collections.Generic;
using Microsoft.Ajax.Utilities;

namespace TicketManagement.Management
{
    public static class NotificationMessages
    {
        private static readonly Dictionary<RoleNotificationType, string> NotificationMessage = new Dictionary<RoleNotificationType, string>
        {
            { RoleNotificationType.PendingApproval, "The user is awaiting approval before being allowed to login." },
            { RoleNotificationType.PendingInternalApproval, "The user is awaiting approval to be confirmed as an internal user." }
        };

        public static string GetMessageOrNull(RoleNotificationType type)
        {
            string notificationMessage;

            NotificationMessage.TryGetValue(type, out notificationMessage);

            return notificationMessage.IsNullOrWhiteSpace() ? null : notificationMessage;
        }
    }
}
