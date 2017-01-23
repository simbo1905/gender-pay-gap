using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenderPayGap.Tests._3.DataInput
{
    [TestFixture]
    public class DataInputTest
    {
        [SetUp]
        public void Setup()
        {
            //this.SetupTest(session => // the NH/EF session to attach the objects to
            //{
            //    var user = new UserAccount("Mr", "Joe", "Bloggs");
            //    session.Save(user);
            //    this.UserID = user.UserID;
            //});
           
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
