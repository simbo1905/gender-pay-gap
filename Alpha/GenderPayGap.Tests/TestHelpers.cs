using Autofac;
using GenderPayGap.Core.Interfaces;
using GpgDB.Models.GpgDatabase;
using IdentityServer3.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GenderPayGap.Tests
{
    public static class TestHelper
    {
        public static T GetController<T>(long userId=0, params object[] dbObjects) where T : Controller
        {
            var builder = BuildContainerIoC(dbObjects);

            //Mock UserId as claim
            var claims = new List<Claim>();

            if (userId>0)claims.Add(new Claim(Constants.ClaimTypes.ExternalProviderUserId, userId.ToString()));
            
            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId>0)mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());

            //Mock HttpContext
            var contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);

            //Mock the httpcontext to the controllercontext
            var controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Setup(con => con.HttpContext).Returns(contextMock.Object);

            T controller = (T)Activator.CreateInstance(typeof(T), builder);
            controller.ControllerContext = controllerContextMock.Object;
            return controller;
        }

        public static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();

            //var mockUnitOfWork = new Mock<IUnitOfWork>();
            //builder.RegisterInstance(mockUnitOfWork.Object).As<IUnitOfWork>();

            //Create the mock repository
            builder.Register(c => new MockRepository(dbObjects)).As<IRepository>();


            return builder.Build();
        }
    }

    public class MockRepository : IRepository
    {
        private readonly List<object> context=new List<object>();

        public MockRepository()
        {
            
        }
        public MockRepository(IEnumerable<object> context)
        {
            if (context != null)this.context = context.ToList();
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return context.OfType<TEntity>().AsQueryable();
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            context.Add(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            context.Remove(entity);
        }


        public void SaveChanges()
        {
        }

        public void Dispose()
        {
        }
    }
}
