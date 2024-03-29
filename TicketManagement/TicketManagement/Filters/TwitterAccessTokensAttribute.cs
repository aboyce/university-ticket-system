﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TicketManagement.Management;
using TicketManagement.Models.Entities;

namespace TicketManagement.Filters
{

    public class TwitterAccessTokensAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ApplicationUserManager userManager = filterContext.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (userManager != null)
            {
                Task<IList<Claim>> claimsForUser = userManager.GetClaimsAsync(filterContext.HttpContext.User.Identity.GetUserId());

                Claim verifierCodeClaim = claimsForUser.Result.FirstOrDefault(c => c.Type == SocialMediaItem.TwitterVerifierCode);
                Claim authorisationIdClaim = claimsForUser.Result.FirstOrDefault(c => c.Type == SocialMediaItem.TwitterAuthorisationId);
                Claim accessTokenClaim = claimsForUser.Result.FirstOrDefault(c => c.Type == SocialMediaItem.TwitterAccessToken);
                Claim accessTokenSecretClaim = claimsForUser.Result.FirstOrDefault(c => c.Type == SocialMediaItem.TwitterAccessTokenSecret);

                if (verifierCodeClaim != null)
                {
                    string verifierCode = verifierCodeClaim.Value;

                    if (filterContext.HttpContext.Items.Contains(SocialMediaItem.TwitterVerifierCode))
                        filterContext.HttpContext.Items[SocialMediaItem.TwitterVerifierCode] = verifierCode;
                    else
                        filterContext.HttpContext.Items.Add(SocialMediaItem.TwitterVerifierCode, verifierCode);
                }

                if (authorisationIdClaim != null)
                {
                    string authorisationId = authorisationIdClaim.Value;

                    if (filterContext.HttpContext.Items.Contains(SocialMediaItem.TwitterAuthorisationId))
                        filterContext.HttpContext.Items[SocialMediaItem.TwitterAuthorisationId] = authorisationId;
                    else
                        filterContext.HttpContext.Items.Add(SocialMediaItem.TwitterAuthorisationId, authorisationId);
                }

                if (accessTokenClaim != null)
                {
                    string accessToken = accessTokenClaim.Value;

                    if (filterContext.HttpContext.Items.Contains(SocialMediaItem.TwitterAccessToken))
                        filterContext.HttpContext.Items[SocialMediaItem.TwitterAccessToken] = accessToken;
                    else
                        filterContext.HttpContext.Items.Add(SocialMediaItem.TwitterAccessToken, accessToken);
                }

                if (accessTokenSecretClaim != null)
                {
                    string accessTokenSecret = accessTokenSecretClaim.Value;

                    if (filterContext.HttpContext.Items.Contains(SocialMediaItem.TwitterAccessTokenSecret))
                        filterContext.HttpContext.Items[SocialMediaItem.TwitterAccessTokenSecret] = accessTokenSecret;
                    else
                        filterContext.HttpContext.Items.Add(SocialMediaItem.TwitterAccessTokenSecret, accessTokenSecret);
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
