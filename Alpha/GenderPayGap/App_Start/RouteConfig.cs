﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GenderPayGap
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "RegisterDefault",
                "{controller}/{action}",
                new { controller = "Register", action = "Step1" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            #region ReturnRoute

            routes.MapRoute(
                   name: "ReturnEdit",
                   url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Return", action = "Submit", id = UrlParameter.Optional }
                );

                //routes.MapRoute(
                //    name: "Default",
                //    url: "{controller}/{action}/{id}",
                //    defaults: new { controller = "Return", action = "Index", id = UrlParameter.Optional }
                //);

                routes.MapRoute(
                 name: "ReturnDetails",
                 url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Return", action = "Details", id = UrlParameter.Optional }
              );


            #endregion

        }
    }
}
