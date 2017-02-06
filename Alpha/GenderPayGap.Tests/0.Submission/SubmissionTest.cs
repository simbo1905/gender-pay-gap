using GenderPayGap.Controllers;
using GenderPayGap.Models;
using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using System.Web;
using System.Security.Principal;
using System.Web.Routing;

namespace GenderPayGap.Tests.Submission
{

    public class MockHttpContext : HttpContextBase
    {
        private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("2"), null /* roles */);
        public override IPrincipal User
        {
            get { return _user; }
            set { base.User = value; }
        }
    }


    [TestFixture]
    public class SubmissionTest
    {

        #region Helper Method
        private HttpContext GetHttpContext()
        {
            throw new NotImplementedException();
        }

        private User GetLoggedInUser()
        {
            throw new NotImplementedException();
        }
        #endregion

        [SetUp]
        public void Setup()
        {
            //Call required helper methods
            HttpContext httpContext = GetHttpContext();
            User currUser = GetLoggedInUser();
        }

        //PAGE LOAD TEST
        //View Load Test:
        //1.User clicks the start page

        //Model is Empty Test:
        //Verify model is empty and form fields are empty test
        //2.Verify the model does not load any data into the form fields

        //User Logged in Test:
        //if user is not logged in redirect to login page test
        //3.page check a database for the user if user Iprinciple is not set or null, redirect to login page

        //User exception mock testing
        //4.if principle is set and principle not exist in the mock, throw an exception

        //User unconfirmed:
        //5.if user exist in the database but have not been verified / confirmed(confirmed email)

        //User unassociated:
        //6.if user exist in the database but have not been been associated with an organisation


    }
}
