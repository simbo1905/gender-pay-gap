using Autofac;
using Extensions;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Classes;
using GpgDB.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace GenderPayGap
{
    public class BaseController:Controller
    {
        public BaseController():this(MvcApplication.ContainerIOC)
        {

        }

        public BaseController(IContainer containerIOC)
        {
            this.containerIOC = containerIOC;
        }

        protected IContainer containerIOC;

        IRepository _Repository;
        protected IRepository Repository
        {
            get
            {
                if (_Repository==null)_Repository = containerIOC.Resolve<IRepository>();
                return _Repository;
            }
        }

        public User GetCurrentUser()
        {
            //Check the ViewBag first 
            if (ViewData.ContainsKey("currentUser")) return (User)ViewData["currentUser"];

            return Repository.FindUser(User);
        }

        public bool Authorise()
        {
            var user = Repository.FindUser(User); //TODO:There is BUG Here
            if (user == null || user.EmailVerifiedDate == null || user.EmailVerifiedDate == DateTime.MinValue)
                return false;

            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.UserId == user.UserId);
            if (userOrg == null || userOrg.PINConfirmedDate == null || userOrg.PINConfirmedDate == DateTime.MinValue)
                return false;

            return true;
        }
    }
}