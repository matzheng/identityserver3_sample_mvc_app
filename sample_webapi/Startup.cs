using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;
using System.Security.Claims;
using System.Linq;

[assembly: OwinStartup(typeof(sample_webapi.Startup))]

namespace sample_webapi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44399/identity",
                RequiredScopes = new [] { "sampleApi"}
            });
            // add app local claims per request
            app.UseClaimsTransformation(incoming =>
            {
                // either add claims to incoming, or create new principal
                var appPrincipal = new ClaimsPrincipal(incoming);
                incoming.Identities.First().AddClaim(new Claim("appSpecific", "some_value"));

                return Task.FromResult(appPrincipal);
            });
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }
    }
}
