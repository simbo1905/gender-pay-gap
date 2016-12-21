﻿using GenderPayGap.Models.GpgDatabase;
using GenderPayGap.Models.GpgEntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GenderPayGap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static GpgDatabase Database = new GpgDatabase();

        protected void Application_Start()
        {
            Database.User.FirstOrDefault();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
