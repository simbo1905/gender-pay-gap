using Autofac;
using Extensions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Models.SqlDatabase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GenderPayGap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IContainer ContainerIOC;

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
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // Process exception


            if (HttpContext.Current.IsCustomErrorEnabled)
            {
                var raisedException = Server.GetLastError();
                if (raisedException is HttpException)
                    HttpContext.Current.Response.Redirect("~/Error/HttpError?code=" + ((HttpException) raisedException).GetHttpCode());
                else
                    HttpContext.Current.Response.Redirect("~/Error/DefaultError");
            }
        }

        public static IContainer BuildContainerIoC()
        {
            var builder = new ContainerBuilder();

            //builder.RegisterType<GpgDatabase>().As<IDbContext>();
            //builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
            builder.Register(c => new SqlRepository(new DbContext())).As<IRepository>();

            return builder.Build();
        }


    }
}
