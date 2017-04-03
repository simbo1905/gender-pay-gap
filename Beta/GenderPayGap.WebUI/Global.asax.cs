using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GenderPayGap.WebUI.Properties;
using GenderPayGap.WebUI.Classes;
using IdentityServer3.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace GenderPayGap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IContainer ContainerIOC;
        public static IFileRepository FileRepository;

        private static Logger _InfoLog;
        public static Logger InfoLog
        {
            get
            {
                if (_InfoLog == null) _InfoLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "InfoLog.txt"));
                return _InfoLog;
            }
        }

        private static Logger _WarningLog;
        public static Logger WarningLog
        {
            get
            {
                if (_WarningLog == null) _WarningLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer","WarningLog.txt"));
                return _WarningLog;
            }
        }

        private static Logger _ErrorLog;
        public static Logger ErrorLog
        {
            get
            {
                if (_ErrorLog == null) _ErrorLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "ErrorLog.txt"));
                return _ErrorLog;
            }
        }

        private static Logger _FeedbackLog;
        public static Logger FeedbackLog
        {
            get
            {
                if (_FeedbackLog == null) _FeedbackLog = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer", "FeedbackLog.csv"));
                return _FeedbackLog;
            }
        }

        private static TelemetryClient _AppInsightsClient;
        public static TelemetryClient AppInsightsClient
        {
            get
            {
                if (_AppInsightsClient==null && !string.IsNullOrWhiteSpace(TelemetryConfiguration.Active.InstrumentationKey) && !TelemetryConfiguration.Active.DisableTelemetry)
                    _AppInsightsClient = new TelemetryClient();
                return _AppInsightsClient;
            }
        }

        public static string AdminEmails = ConfigurationManager.AppSettings["AdminEmails"];
        public static string TestPrefix = ConfigurationManager.AppSettings["TestPrefix"];
        public static bool MaintenanceMode= ConfigurationManager.AppSettings["MaintenanceMode"].ToBoolean();
        public static bool StickySessions = ConfigurationManager.AppSettings["StickySessions"].ToBoolean(true);
        public static bool EncryptEmails = ConfigurationManager.AppSettings["EncryptEmails"].ToBoolean(true);
        public static bool EnableSubmitAlerts = ConfigurationManager.AppSettings["EnableSubmitAlerts"].ToBoolean();

        /// <summary>
        /// Return true if exactly one concrete admin defined 
        /// </summary>
        public static bool SingleAdminMode
        {
            get
            {
                var args = AdminEmails.SplitI(";");
                return args.Length == 1 && !string.IsNullOrWhiteSpace(args[0]) && !args[0].ContainsAny('*', '?') &&
                       args[0].IsEmailAddress();
            }
        }


        protected void Application_Start()
        {
#if DEBUG
            var context = new DbContext();
            context.User.FirstOrDefault();//Test entity framework loads ok
#endif
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        
            //Create Inversion of Control container
            ContainerIOC = BuildContainerIoC();
            FileRepository = ContainerIOC.Resolve<IFileRepository>();

            //Initialise static classes with IoC container
            GovNotifyAPI.Initialise(ContainerIOC);

            RouteTable.Routes.MapMvcAttributeRoutes();

            //Set the machine key
            SetMachineKey();

            //Set Application Insights instrumentation key
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];

            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //Redirect to holding mage if in maintenance mode
            if (MaintenanceMode && !HttpContext.Current.Request.Url.PathAndQuery.StartsWithI(@"/Error/service-unavailable")) HttpContext.Current.Response.Redirect(@"/Error/service-unavailable",true);

            //Disable sticky sessions
            if (!StickySessions) Response.Headers.Add("Arr-Disable-Session-Affinity", "True");
        }

        protected void Application_Error(Object sender, EventArgs e)
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
                HttpContext.Current.Response.Redirect("~/Error?code=" + ((HttpException) raisedException).GetHttpCode());
            else
                HttpContext.Current.Response.Redirect("~/Error");

            //Track the exception with Application Insights if it is available
            AppInsightsClient?.TrackException(raisedException);
        }
        
        public static IContainer BuildContainerIoC()
        {
            var builder = new ContainerBuilder();

            //builder.RegisterType<GpgDatabase>().As<IDbContext>();
            //builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
            builder.Register(c => new SqlRepository(new DbContext())).As<IRepository>();
            builder.RegisterType<PrivateSectorRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Private");
            builder.RegisterType<PublicSectorRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Public");
            builder.Register(g => new GovNotify()).As<IGovNotify>();

            var azureStorageConnectionString = ConfigurationManager.AppSettings["AzureStorageConnectionString"];
            var azureStorageShareName = ConfigurationManager.AppSettings["AzureStorageShareName"];
            var localStorageRoot = ConfigurationManager.AppSettings["LocalStorageRoot"];

            if (!string.IsNullOrWhiteSpace(azureStorageConnectionString) && !string.IsNullOrWhiteSpace(azureStorageShareName))
                builder.Register(c => new AzureFileRepository(azureStorageConnectionString, azureStorageShareName)).As<IFileRepository>();
            else
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>();

            return builder.Build();
        }

        protected void Session_Start() { }

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
