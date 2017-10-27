using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using IdentityServer3.Core;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Google;
using IdentityModel.Client;
using System.Linq;

[assembly: OwinStartup(typeof(sample_webapp.Startup))]

namespace sample_webapp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.Map("/identity", idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(new IdentityServerOptions
                        {
                            SiteName = "Embedded Identity Server",
                            SigningCertificate = LoadCertificate(),
                            Factory = new IdentityServerServiceFactory()
                                    .UseInMemoryUsers(Users.Get())
                                    .UseInMemoryClients(Clients.Get())
                                    //.UseInMemoryScopes(StandardScopes.All)
                                    .UseInMemoryScopes(Scopes.Get()),
                            AuthenticationOptions = new IdentityServer3.Core.Configuration.AuthenticationOptions
                            {
                                EnablePostSignOutAutoRedirect = true,
                                IdentityProviders = ConfigureIdentityProviders
                            }
                        }
                    );
                }
            );

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseResourceAuthorization(new AuthorizationManager());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Authority = "https://localhost:44399/identity",
                    ClientId = "mvc",
                    RedirectUri = "https://localhost:44399/",
                    ResponseType = "id_token token",
                    SignInAsAuthenticationType = "Cookies",
                    Scope = "openid profile roles sampleApi",
                    UseTokenLifetime = false ,
                #region Notifications
                    
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {                       

                        var nid = new ClaimsIdentity(
                            n.AuthenticationTicket.Identity.AuthenticationType,
                            Constants.ClaimTypes.GivenName,
                            Constants.ClaimTypes.Role);

                        // get userinfo data
                        var userInfoClient = new UserInfoClient(
                        new Uri(n.Options.Authority + "/connect/userinfo"),
                        n.ProtocolMessage.AccessToken);

                        var userInfo = await userInfoClient.GetAsync();
                        userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Item1, ui.Item2)));

                        // keep the id_token for logout
                        nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

                        // add access token for sample API
                        nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                        // keep track of access token expiration
                        nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

                        // add some other app specific claim
                        nid.AddClaim(new Claim("app_specific", "some data"));

                        n.AuthenticationTicket = new AuthenticationTicket(
                            nid,
                            n.AuthenticationTicket.Properties);
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }
                        }

                        return Task.FromResult(0);
                    }
                }
                
                #endregion
            }
            );

           
        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test"
            );
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                    AuthenticationType = "Google",
                    Caption = "Sign-In with Google plus",
                    SignInAsAuthenticationType = signInAsType,
                    ClientId = "591574956975-s2aiekrtdkhgl9nqiqbr6ig6ai1mbgv7.apps.googleusercontent.com",
                    ClientSecret = "U7BpOJVsm2wUq7ZTaQYCGYev"
            }
            );
        }
    }
}
