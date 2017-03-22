using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;

namespace GpgIdentityServer
{
    public class Global : System.Web.HttpApplication
    {

        public static IContainer ContainerIOC;
        public static IFileRepository FileRepository;

        private static Logger _Log;
        public static Logger Log
        {
            get
            {
                if (_Log == null) _Log = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"],"IdentityServer"));
                return _Log;
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {

            //Create Inversion of Control container
            ContainerIOC = BuildContainerIoC();
            FileRepository = ContainerIOC.Resolve<IFileRepository>();

            //Set the machine key
            SetMachineKey();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start()
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //Redirect to holding mage if in maintenance mode
            if (ConfigurationManager.AppSettings["MaintenanceMode"].ToBoolean())
                HttpContext.Current.Response.Redirect(@"/Error/service-unavailable");
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }

        public static IContainer BuildContainerIoC()
        {
            var builder = new ContainerBuilder();

            var azureStorageConnectionString = ConfigurationManager.AppSettings["AzureStorageConnectionString"];
            var azureStorageShareName = ConfigurationManager.AppSettings["AzureStorageShareName"];
            var localStorageRoot = ConfigurationManager.AppSettings["LocalStorageRoot"];

            if (!string.IsNullOrWhiteSpace(azureStorageConnectionString) && !string.IsNullOrWhiteSpace(azureStorageShareName))
                builder.Register(c => new AzureFileRepository(azureStorageConnectionString, azureStorageShareName)).As<IFileRepository>();
            else
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>();

            return builder.Build();
        }

        void SetMachineKey()
        {
            var mksType = typeof(MachineKeySection);
            var mksSection = ConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;
            var resetMethod = mksType.GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Instance);

            var newConfig = new MachineKeySection();
            newConfig.ApplicationName = mksSection.ApplicationName;
            newConfig.CompatibilityMode = mksSection.CompatibilityMode;
            newConfig.DataProtectorType = mksSection.DataProtectorType;
            newConfig.Validation = mksSection.Validation;

            newConfig.ValidationKey = ConfigurationManager.AppSettings["MK_ValidationKey"];
            newConfig.DecryptionKey = ConfigurationManager.AppSettings["MK_DecryptionKey"];
            newConfig.Decryption = ConfigurationManager.AppSettings["MK_Decryption"]; // default: AES
            newConfig.ValidationAlgorithm = ConfigurationManager.AppSettings["MK_ValidationAlgorithm"]; // default: SHA1

            resetMethod.Invoke(mksSection, new object[] { newConfig });
        }

    }
}