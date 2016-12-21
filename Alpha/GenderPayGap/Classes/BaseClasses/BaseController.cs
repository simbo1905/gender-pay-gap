using Extensions;
using GenderPayGap.Models.GpgDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
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

            return User.FindCurrentUser();
        }
    }
}