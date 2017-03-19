using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Models.SqlDatabase;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GenderPayGap.WebUI.Properties;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IContainer ContainerIOC;
        public static IFileRepository FileRepository;

        private static Logger _Log;
        public static Logger Log
        {
            get
            {
                if (_Log == null) _Log = new Logger(FileRepository, Path.Combine(ConfigurationManager.AppSettings["LogPath"], "WebServer"));
                return _Log;
            }
        }

        public static string AdminEmails = ConfigurationManager.AppSettings["AdminEmails"];
        public static bool MaintenanceMode= ConfigurationManager.AppSettings["MaintenanceMode"].ToBoolean();

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

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // Process exception
            if (HttpContext.Current.IsCustomErrorEnabled)
            {
                var raisedException = Server.GetLastError();
                if (raisedException != null)
                {
                    //Add to the log
                    Log.WriteLine(raisedException.ToString());

                    if (raisedException is HttpException)
                        HttpContext.Current.Response.Redirect("~/Error?code=" + ((HttpException) raisedException).GetHttpCode());
                    else
                        HttpContext.Current.Response.Redirect("~/Error");
                }
            }
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

    }
}
