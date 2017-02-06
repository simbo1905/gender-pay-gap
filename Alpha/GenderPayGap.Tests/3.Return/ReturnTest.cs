using GenderPayGap.Models.GpgDatabase;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GenderPayGap.Tests._3.DataInput
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
    public class ReturnTest
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


        [Test]
        [Description("This test is to verify that the user successfully logged in")]
        public void VerifyLoginPassed()
        {
            Assert.That(false, "Error Message");
        }

        [Test]
        public void TestTitle()
        {
            //var user = LoadMyUser(this.UserID); // load the entity
            //Assert.AreEqual("Mr", user.Title);
            Assert.That(false, "Error Message");
        }

        [Test]
        public void TestFirstname()
        {
            //var user = LoadMyUser(this.UserID);
            //Assert.AreEqual("Joe", user.Firstname);
            Assert.That(false, "Error Message");
        }

        [Test]
        public void TestLastname()
        {
            //var user = LoadMyUser(this.UserID);
            //Assert.AreEqual("Bloggs", user.Lastname);
            Assert.That(false, "Error Message");
        }

        [TearDown]
        public void TearDown()
        {
            //Tear down code here!
        }
    }
}
