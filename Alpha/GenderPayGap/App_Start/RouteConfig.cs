using System;
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
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            #region ReturnRoute

            routes.MapRoute(
                   name: "ReturnEdit",
                   url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Return", action = "Edit", id = UrlParameter.Optional }
                );

                //routes.MapRoute(
                //    name: "Default",
                //    url: "{controller}/{action}/{id}",
                //    defaults: new { controller = "Return", action = "Index", id = UrlParameter.Optional }
                //);

                routes.MapRoute(
                   name: "ReturnCreate",
                   url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Return", action = "Create", id = UrlParameter.Optional }
                );

                routes.MapRoute(
                  name: "ReturnDelete",
                  url: "{controller}/{action}/{id}",
                  defaults: new { controller = "Return", action = "Delete", id = UrlParameter.Optional }
               );

                routes.MapRoute(
                 name: "ReturnDetails",
                 url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Return", action = "Details", id = UrlParameter.Optional }
              );


            #endregion

        }
    }
}
