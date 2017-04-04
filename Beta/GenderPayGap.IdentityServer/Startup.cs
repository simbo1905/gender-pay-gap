﻿using Microsoft.Owin;
using Owin;
using IdentityServer3.Core.Configuration;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;

[assembly: OwinStartup(typeof(GenderPayGap.IdentityServer.Startup))]

namespace GenderPayGap.IdentityServer
{
    public class Startup
    {
        static CustomLogProvider Logger=new CustomLogProvider();
        public void Configuration(IAppBuilder app)
        {
            LogProvider.SetCurrentLogProvider(new CustomLogProvider());

            app.Map("/login", coreApp =>
            {
                
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    //.UseInMemoryUsers(Users.Get())
                    .UseInMemoryScopes(Scopes.Get());

                //Set the options for the default view service
                var viewOptions = new DefaultViewServiceOptions();
#if DEBUG
                //Dont cache the views when we are testing
                viewOptions.CacheViews = false;
#endif
                factory.ConfigureDefaultViewService(viewOptions);

                // different examples of custom user services
                //var userService = new RegisterFirstExternalRegistrationUserService();
                //var userService = new ExternalRegistrationUserService();
                //var userService = new EulaAtLoginUserService();
                var userService = new LocalRegistrationUserService();

                // note: for the sample this registration is a singletone (not what you want in production probably)
                factory.UserService = new Registration<IUserService>(resolver => userService);

                //Required for GPG custom interface
                //factory.ViewService = new Registration<IViewService, CustomViewService>();

                factory.EventService = new Registration<IEventService, AuditEventService>();

                var options = new IdentityServerOptions
                {
                    SiteName = "GPG IdentityServer",
                    SigningCertificate = LoadCertificate(),
                    Factory = factory,

                    AuthenticationOptions = new AuthenticationOptions
                    {
                        EnablePostSignOutAutoRedirect = true,
                        IdentityProviders = ConfigureIdentityProviders,
                        
                        EnableSignOutPrompt = false,
                        InvalidSignInRedirectUrl = ConfigurationManager.AppSettings["GpgWebServer"],
                        
                        LoginPageLinks = new List<LoginPageLink>()
                        {
                           new LoginPageLink()
                           {
                               Href = ConfigurationManager.AppSettings["GpgWebServerPasswordLink"],
                               Text = "Reset your password",
                               Type = "resetPassword"
                           },
                           new LoginPageLink()
                           {
                               Href = ConfigurationManager.AppSettings["GpgWebServerRegisterLink"],
                               Text = "Register",
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

            //app.Map("/login", idsrvApp =>
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
            if (string.IsNullOrWhiteSpace(MvCApplication.CertThumprint)) return new X509Certificate2(string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");

            X509Certificate2 cert = null;
            using (var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);

                //Try and get a valid cert
                var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, MvCApplication.CertThumprint, true);
                //Otherwise use an invalid cert
                if (certCollection.Count == 0) certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, MvCApplication.CertThumprint, false);

                if (certCollection.Count > 0)cert=certCollection[0];

                certStore.Close();
            }

            if (cert==null)throw new Exception($"Cannot find certificate with thumbprint '{MvCApplication.CertThumprint}' in local store");
            MvCApplication.InfoLog.WriteLine($"Successfully loaded certificate from thumbprint {MvCApplication.CertThumprint}");
            return cert;
        }
    }
}