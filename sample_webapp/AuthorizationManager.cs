using IdentityServer3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace sample_webapp
{
    public class AuthorizationManager : ResourceAuthorizationManager
    {
        public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            switch(context.Resource.First().Value)
            {
                case "ContactDetails":
                    return AuthorizeContactDetails(context);
                default:
                    return Nok();
            }
        }

        private Task<bool> AuthorizeContactDetails(ResourceAuthorizationContext context)
        {
            switch(context.Action.First().Value)
            {
                case "Read":
                    return Eval(context.Principal.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Geek"));
                case "Write":
                    return Eval(context.Principal.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Operator"));
                default:
                    return Nok();
            }
        }
    }
}