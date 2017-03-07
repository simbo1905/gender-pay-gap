using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Extensions;

namespace GpgIdentityServer
{
    public class Global : System.Web.HttpApplication
    {
        static Extensions.Logger _Logger;

        public static Extensions.Logger Logger
        {
            get
            {
                if (_Logger == null) _Logger = new Extensions.Logger(FileSystem.ExpandLocalPath(Path.Combine(ConfigurationManager.AppSettings["LogPath"], "Errors")));
                return _Logger;
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start() { }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

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
    }
}