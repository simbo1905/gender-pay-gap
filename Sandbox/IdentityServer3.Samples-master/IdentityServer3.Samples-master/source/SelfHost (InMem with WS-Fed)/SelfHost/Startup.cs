﻿using Owin;
using SelfHost.Config;
using System.Collections.Generic;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Host.Config;
using IdentityServer3.WsFederation.Configuration;
using IdentityServer3.WsFederation.Models;
using IdentityServer3.WsFederation.Services;
using Serilog;

namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.LiterateConsole(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
               .CreateLogger();
            
            var factory = new IdentityServerServiceFactory()
                            .UseInMemoryUsers(Users.Get())
                            .UseInMemoryClients(Clients.Get())
                            .UseInMemoryScopes(Scopes.Get());

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 - WsFed",

                SigningCertificate = Certificate.Get(),
                Factory = factory,
                PluginConfiguration = ConfigurePlugins,
            };

            appBuilder.UseIdentityServer(options);
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var wsFedOptions = new WsFederationPluginOptions(options);

            // data sources for in-memory services
            wsFedOptions.Factory.Register(new Registration<IEnumerable<RelyingParty>>(RelyingParties.Get()));
            wsFedOptions.Factory.RelyingPartyService = new Registration<IRelyingPartyService>(typeof(InMemoryRelyingPartyService));

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }
    }
}