using Microsoft.Owin;
using Owin;
using IdentityServer3.Core.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Owin.Security.Google;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Owin.Security.Twitter;
using IdentityServer3.Core.Services;
using Microsoft.Owin.Security.Facebook;

[assembly: OwinStartup(typeof(GpgIdentityServer.Startup))]

namespace GpgIdentityServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/identity", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    //.UseInMemoryUsers(Users.Get())
                    .UseInMemoryScopes(Scopes.Get());

                // different examples of custom user services
                //var userService = new RegisterFirstExternalRegistrationUserService();
                //var userService = new ExternalRegistrationUserService();
                //var userService = new EulaAtLoginUserService();
                var userService = new LocalRegistrationUserService();

                // note: for the sample this registration is a singletone (not what you want in production probably)
                factory.UserService = new Registration<IUserService>(resolver => userService);

                //factory.ViewService = new Registration<IViewService, CustomViewService>();

                var options = new IdentityServerOptions
                {
                    SiteName = "GPG IdentityServer",
                    SigningCertificate = LoadCertificate(),

                    Factory = factory,

                    AuthenticationOptions = new AuthenticationOptions
                    {
                        EnablePostSignOutAutoRedirect = true,
                        IdentityProviders = ConfigureIdentityProviders,
                        LoginPageLinks = new List<LoginPageLink>()
                        {
                           new LoginPageLink()
                           {
                               Href = ConfigurationManager.AppSettings["GpgWebServerReminder"],
                               Text = "Forgotten Password?",
                               Type = "resetPassword"
                           },
                           new LoginPageLink()
                           {
                               Href = ConfigurationManager.AppSettings["GpgWebServerRegister"],
                               Text = "Create New Account",
                               Type = "localRegistration"
                           }
                        }
                    },

                    EventsOptions = new EventsOptions
                    {
                        RaiseSuccessEvents = true,
                        RaiseErrorEvents = true,
                        RaiseFailureEvents = true,
                        RaiseInformationEvents = true
                    }
                };

                coreApp.UseIdentityServer(options);
            });

            //app.Map("/identity", idsrvApp =>
            //{
            //    idsrvApp.UseIdentityServer(new IdentityServerOptions
            //    {
            //        SiteName = "GPG IdentityServer",
            //        SigningCertificate = LoadCertificate(),

            //        Factory = new IdentityServerServiceFactory()
            //                    .UseInMemoryUsers(Users.Get())
            //                    .UseInMemoryClients(Clients.Get())
            //                    .UseInMemoryScopes(Scopes.Get()),

            //        AuthenticationOptions = new IdentityServer3.Core.Configuration.AuthenticationOptions
            //        {
            //            EnablePostSignOutAutoRedirect = true,
            //            IdentityProviders = ConfigureIdentityProviders,
            //            LoginPageLinks = new List<LoginPageLink>()
            //            {
            //               new LoginPageLink()
            //               {
            //                   Href = ConfigurationManager.AppSettings["GpgWebServerReminder"],
            //                   Text = "Forgotten Password?",
            //                   Type = "resetPassword"
            //               },
            //               new LoginPageLink()
            //               {
            //                   Href = ConfigurationManager.AppSettings["GpgWebServerRegister"],
            //                   Text = "Create New Account",
            //                   Type = "localRegistration"
            //               }
            //            }

            //        }
            //    });


            //});
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            //{
            //    AuthenticationType = "Google",
            //    Caption = "Sign-in with Google",
            //    SignInAsAuthenticationType = signInAsType,

            //    //ClientId = "701386055558-9epl93fgsjfmdn14frqvaq2r9i44qgaa.apps.googleusercontent.com",
            //    //ClientSecret = "3pyawKDWaXwsPuRDL7LtKm_o"

            //    ClientId = "979363520284-3l9brf97vd16jvv68je6of8b7v8ej48d.apps.googleusercontent.com",
            //    ClientSecret = "Rp8_erHf2xZmJInm35iC7ug_"
                
            //});

            //var fb = new FacebookAuthenticationOptions
            //{
            //    AuthenticationType = "Facebook",
            //    SignInAsAuthenticationType = signInAsType,
            //    AppId = "676607329068058",
            //    AppSecret = "9d6ab75f921942e61fb43a9b1fc25c63"
            //};
            //app.UseFacebookAuthentication(fb);

            //var twitter = new TwitterAuthenticationOptions
            //{
            //    AuthenticationType = "Twitter",
            //    SignInAsAuthenticationType = signInAsType,
            //    ConsumerKey = "N8r8w7PIepwtZZwtH066kMlmq",
            //    ConsumerSecret = "df15L2x6kNI50E4PYcHS0ImBQlcGIt6huET8gQN41VFpUCwNjM"
            //};
            //app.UseTwitterAuthentication(twitter);

        }

        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}