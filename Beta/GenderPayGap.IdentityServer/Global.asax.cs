using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace GenderPayGap.IdentityServer
{
    public class MvCApplication : System.Web.HttpApplication
    {

        public static IContainer ContainerIOC;
        public static IFileRepository FileRepository;


        private static Logger _InfoLog;
        public static Logger InfoLog
        {
            get
            {
                if (_InfoLog == null) _InfoLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "IdentityServer", "InfoLog.txt"));
                return _InfoLog;
            }
        }

        private static Logger _WarningLog;
        public static Logger WarningLog
        {
            get
            {
                if (_WarningLog == null) _WarningLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "IdentityServer", "WarningLog.txt"));
                return _WarningLog;
            }
        }

        private static Logger _ErrorLog;
        public static Logger ErrorLog
        {
            get
            {
                if (_ErrorLog == null) _ErrorLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "IdentityServer", "ErrorLog.txt"));
                return _ErrorLog;
            }
        }


        private static TelemetryClient _AppInsightsClient;
        public static TelemetryClient AppInsightsClient
        {
            get
            {
                if (_AppInsightsClient == null && !string.IsNullOrWhiteSpace(TelemetryConfiguration.Active.InstrumentationKey) && !TelemetryConfiguration.Active.DisableTelemetry)
                    _AppInsightsClient = new TelemetryClient();
                return _AppInsightsClient;
            }
        }

        public static string AdminEmails = ConfigurationManager.AppSettings["AdminEmails"];
        public static string TrustedIPDomains = ConfigurationManager.AppSettings["TrustedIPDomains"];
        public static bool MaintenanceMode = ConfigurationManager.AppSettings["MaintenanceMode"].ToBoolean();
        public static bool StickySessions = ConfigurationManager.AppSettings["StickySessions"].ToBoolean(true);
        public static bool EncryptEmails = ConfigurationManager.AppSettings["EncryptEmails"].ToBoolean(true);

        protected void Application_Start(object sender, EventArgs e)
        {

            //Create Inversion of Control container
            ContainerIOC = BuildContainerIoC();
            FileRepository = ContainerIOC.Resolve<IFileRepository>();

            //Set the machine key
            SetMachineKey();

            //Set Application Insights instrumentation key
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
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
            if (MaintenanceMode)HttpContext.Current.Response.Redirect(@"/Error/service-unavailable", true);

            //Disable sticky sessions
            if (!StickySessions) Response.Headers.Add("Arr-Disable-Session-Affinity", "True");
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Process exception
            if (!HttpContext.Current.IsCustomErrorEnabled) return;
            var raisedException = Server.GetLastError();
            if (raisedException == null) return;

            //Add to the log
            ErrorLog.WriteLine(raisedException.ToString());

            // Note: A single instance of telemetry client is sufficient to track multiple telemetry items.

            var ai = new TelemetryClient();
            ai.TrackException(raisedException);

            if (raisedException is HttpException)
                HttpContext.Current.Response.Redirect("/Error?code=" + ((HttpException)raisedException).GetHttpCode());
            else
                HttpContext.Current.Response.Redirect("/Error");

            //Track the exception with Application Insights if it is available
            AppInsightsClient?.TrackException(raisedException);
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