using Autofac;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Models.SqlDatabase;
using GenderPayGap.WebUI.Models;
using IdentityServer3.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GenderPayGap.Core.Classes;
using Extensions;

namespace GenderPayGap.Tests
{
    public static class TestHelper
    {
        private const string Url = "https://genderpaygap.azurewebsites.net";
        

        public static T GetController<T>(long userId = 0, RouteData routeData = null, params object[] dbObjects) where T : Controller
        {
            var builder = BuildContainerIoC(dbObjects);

            //Mock UserId as claim
            var claims = new List<Claim>();

            if (userId > 0) claims.Add(new Claim(Constants.ClaimTypes.Subject, userId.ToString()));

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId > 0) mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());

            //Mock HttpRequest
            var requestMock = new Mock<HttpRequestBase>();
            requestMock.SetupGet(x => x.ApplicationPath).Returns("/");
            requestMock.SetupGet(x => x.Url).Returns(new Uri(Url, UriKind.Absolute));
            requestMock.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());

            //Mock HttpResponse
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string url) => url);

            //Mock session
            var sessionMock = new MockHttpSession();

            //Mock HttpContext
            var contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
            contextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            contextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            contextMock.Setup(ctx => ctx.Session).Returns(sessionMock);

            //Mock the httpcontext to the controllercontext
            var controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Setup(con => con.HttpContext).Returns(contextMock.Object);
            controllerContextMock.Setup(con => con.RouteData).Returns(routeData);
            if (routeData == null) routeData = new RouteData();
            T controller = (T)Activator.CreateInstance(typeof(T), builder);
            var routes = new RouteCollection();
            RouteConfig.RegisterRoutes(routes);
            controller.ControllerContext = controllerContextMock.Object;
            controller.Url = new UrlHelper(new RequestContext(contextMock.Object, routeData),routes);
            return controller;
        }

        public class MockHttpSession : HttpSessionStateBase
        {
            Dictionary<string, object> m_SessionStorage = new Dictionary<string, object>(); 

            public override object this[string name]
            {
                get { return m_SessionStorage[name]; }
                set { m_SessionStorage[name] = value; }
            }

            public override void Remove(string name)
            {
                m_SessionStorage.Remove(name);
            }
        }

        public static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();

            //var mockUnitOfWork = new Mock<IUnitOfWork>();
            //builder.RegisterInstance(mockUnitOfWork.Object).As<IUnitOfWork>();

            //Create the mock repository
            builder.Register(c => new MockRepository(dbObjects)).As<IRepository>();
            builder.RegisterType<MockEmployerRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Private");
            builder.RegisterType<MockEmployerRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Public");


            return builder.Build();
        }

        public static void Bind(this Controller controller, object Model)
        {
            ValidationContext validationContext = new ValidationContext(Model, null, null);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(Model, validationContext, validationResults, true);
            foreach (ValidationResult validationResult in validationResults)
            {
                controller.ModelState.AddModelError(String.Join(", ", validationResult.MemberNames), validationResult.ErrorMessage);
            }
        }
    }

    public class MockRepository : IRepository
    {
        private readonly List<object> context = new List<object>();

        public MockRepository()
        {

        }
        public MockRepository(IEnumerable<object> context)
        {
            if (context != null) this.context = context.ToList();
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

    public class MockEmployerRepository : IPagedRepository<EmployerRecord>
    {
        public List<EmployerRecord> AllEmployers = new List<EmployerRecord>();

        public void Delete(EmployerRecord employer)
        {
            AllEmployers.Remove(employer);
        }

        public void Insert(EmployerRecord employer)
        {
            AllEmployers.Add(employer);
        }

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize)
        {
            var result = new PagedResult<EmployerRecord>();
            result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
            result.RowCount = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.PageCount = (int)Math.Ceiling((double)result.RowCount / pageSize);
            return result;
        }

        PagedResult<EmployerRecord> IPagedRepository<EmployerRecord>.Search(string searchText, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }

}
