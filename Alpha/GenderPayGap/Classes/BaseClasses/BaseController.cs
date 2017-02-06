using Extensions;
using GenderPayGap.Models.GpgDatabase;
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
        public User GetCurrentUser()
        {
            //Check the ViewBag first 
            if (ViewData.ContainsKey("currentUser")) return (User)ViewData["currentUser"];
            return GenderPayGap.Models.GpgDatabase.User.FindCurrentUser(User);
        }

        public bool Authorise()
        {
            var user = Models.GpgDatabase.User.FindCurrentUser(User); //TODO:There is BUG Here
            if (user == null || user.EmailVerifiedDate == null || user.EmailVerifiedDate == DateTime.MinValue)
                return false;

            var userOrg = GpgDatabase.Default.UserOrganisations.FirstOrDefault(u => u.UserId == user.UserId);
            if (userOrg == null || userOrg.PINConfirmedDate == null || userOrg.PINConfirmedDate == DateTime.MinValue)
                return false;

            return true;
        }
    }
}