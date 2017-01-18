using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.Tests.Registeration
{
    [TestFixture]
    public class RegistrationTestClass : AssertionHelper
    {
        [Test]
        [Description("When user clicks on the register link from Sign-in, user is taken to register page (1 of 6)")]
        public void RegisterLinkValidation()
        {
            // TODO: Add your test code here
            Assert.Pass("Your first passing test");
            
        }

        [Test, Order(1)]
        [Description("Verify that the user is not already logged in")]
        public void VerifyUserIsNotLoggedIn()
        {
            // TODO: Add your test code here
            Assert.Pass("Your first passing test");
        }

        [Test, Order(2)]
        [Description("Verify that the user does not exist")]
        public void VerifyUserDoesNotExist()
        {
            // TODO: Add your test code here
            Assert.Pass("Your first passing test");
        }

        [Test, Order(3)]
        [Description("Verify that all fields are empty hence not populated")]
        public void VerifyUserFormFieldsAreEmpty()
        {
            // TODO: Add your test code here
            Assert.Pass("Your first passing test");
        }

    }
}
