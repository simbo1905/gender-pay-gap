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
using System.Data;
using System.Data.Common;
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
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Register;
using Notify.Models;

namespace GenderPayGap.Tests
{
    public static class TestHelper
    {
        private const string Url = "https://genderpaygap.azurewebsites.net";
        

        public static T GetController<T>(long userId = 0, RouteData routeData = null, params object[] dbObjects) where T : Controller
        {
            var builder = BuildContainerIoC(dbObjects);

            //Initialise static classes with IoC container
            GovNotifyAPI.Initialise(builder);

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
      
            //Done:Added the queryString Dictionary property for mock object
            var queryString = new NameValueCollection()
                {
                    {"code","abcdefg" }
                };
            //queryString.Add("code", "abcdefg");
            requestMock.SetupGet(x => x.QueryString).Returns(queryString);

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
            MvcApplication.FileRepository = builder.Resolve<IFileRepository>();

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
            builder.RegisterType<MockPrivateEmployerRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Private");
            builder.RegisterType<MockPublicEmployerRepository>().As<IPagedRepository<EmployerRecord>>().Keyed<IPagedRepository<EmployerRecord>>("Public");
            builder.Register(g => new MockGovNotify()).As<IGovNotify>();

            builder.Register(c => new SystemFileRepository()).As<IFileRepository>();
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

        public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }

    public class MockPrivateEmployerRepository : IPagedRepository<EmployerRecord>
    {
        public List<EmployerRecord> AllEmployers = new List<EmployerRecord>();

        public void Delete(EmployerRecord employer)
        {
            AllEmployers.Remove(employer);
        }

        public string GetSicCodes(string companyNumber)
        {
            //throw new NotImplementedException();

            //fakedIt.
            return "13243546576879";
        }

        public void Insert(EmployerRecord employer)
        {
            AllEmployers.Add(employer);
        }

        public PagedResult<EmployerRecord> Search(string searchText, int page, int pageSize)
        {
            var result = new PagedResult<EmployerRecord>();
            //DONE:NastyBug! Page method arguments Page(pageSize, page) where in vice-versa positions as in Page(page, pageSize)! now fixed 
            result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
            result.RowCount = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.PageCount = (int)Math.Ceiling((double)result.RowCount / pageSize);
            return result;
        }

        PagedResult<EmployerRecord> IPagedRepository<EmployerRecord>.Search(string searchText, int page, int pageSize)
        {
          int totalRecords;
         // var searchResults = CompaniesHouseAPI.SearchEmployers(out totalRecords, searchText, page, pageSize);
         var result = new PagedResult<EmployerRecord>()
         {
          Results = new List<EmployerRecord>()
          {
            //new EmployerRecord() { Name = "Acme  Inc", Address1 = "10", Address2 = "EverGreen Terrace", CompanyNumber = "123QA10", CompanyStatus = "Active", Country = "UK", PostCode = "w12  3we" },
            //new EmployerRecord() { Name = "Beano Inc", Address1 = "11", Address2 = "EverGreen Terrace", CompanyNumber = "123QA11", CompanyStatus = "Active", Country = "UK", PostCode = "n12  4qw" },
            //new EmployerRecord() { Name = "Smith ltd", Address1 = "12", Address2 = "EverGreen Terrace", CompanyNumber = "123QA12", CompanyStatus = "Active", Country = "UK", PostCode = "nw2  1de" },
            //new EmployerRecord() { Name = "Trax ltd",  Address1 = "13", Address2 = "EverGreen Terrace", CompanyNumber = "123QA13", CompanyStatus = "Active", Country = "UK", PostCode = "sw2  5gh" },
            //new EmployerRecord() { Name = "Exant ltd", Address1 = "14", Address2 = "EverGreen Terrace", CompanyNumber = "123QA14", CompanyStatus = "Active", Country = "UK", PostCode = "se2  2bh" },
            //new EmployerRecord() { Name = "Serif ltd", Address1 = "15", Address2 = "EverGreen Terrace", CompanyNumber = "123QA15", CompanyStatus = "Active", Country = "UK", PostCode = "da2  6cd" },
            //new EmployerRecord() { Name = "West ltd",  Address1 = "16", Address2 = "EverGreen Terrace", CompanyNumber = "123QA16", CompanyStatus = "Active", Country = "UK", PostCode = "cd2  1cs" },
            //new EmployerRecord() { Name = "North ltd", Address1 = "17", Address2 = "EverGreen Terrace", CompanyNumber = "123QA17", CompanyStatus = "Active", Country = "UK", PostCode = "e12  7xs" },
            //new EmployerRecord() { Name = "South ltd", Address1 = "18", Address2 = "EverGreen Terrace", CompanyNumber = "123QA18", CompanyStatus = "Active", Country = "UK", PostCode = "e17  8za" },
            //new EmployerRecord() { Name = "East ltd",  Address1 = "19", Address2 = "EverGreen Terrace", CompanyNumber = "123QA19", CompanyStatus = "Active", Country = "UK", PostCode = "sw25 9bh" },
            //new EmployerRecord() { Name = "Dax ltd",   Address1 = "20", Address2 = "EverGreen Terrace", CompanyNumber = "123QA20", CompanyStatus = "Active", Country = "UK", PostCode = "se1  6nh" },
            //new EmployerRecord() { Name = "Merty ltd", Address1 = "21", Address2 = "EverGreen Terrace", CompanyNumber = "123QA21", CompanyStatus = "Active", Country = "UK", PostCode = "se32 2nj" },
            //new EmployerRecord() { Name = "Daxam ltd", Address1 = "22", Address2 = "EverGreen Terrace", CompanyNumber = "123QA22", CompanyStatus = "Active", Country = "UK", PostCode = "e1   1nh" },
            //new EmployerRecord() { Name = "Greta ltd", Address1 = "23", Address2 = "EverGreen Terrace", CompanyNumber = "123QA23", CompanyStatus = "Active", Country = "UK", PostCode = "e19  8vt" },
            //new EmployerRecord() { Name = "Buxom ltd", Address1 = "24", Address2 = "EverGreen Terrace", CompanyNumber = "123QA24", CompanyStatus = "Active", Country = "UK", PostCode = "sw1  5ml" },
          }
         };

            //result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
            //TODO: ste -> using this until Page function lines 879 and 888  in Lists.cs is fixed.  
            result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).ToList();

            //DONE:NastyBug! Page method arguments Page(pageSize, page) where in vice-versa positions as in Page(page, pageSize)! now fixed 
            result.RowCount = totalRecords = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            // result.Results = searchResults;
            return result;
        }

    }

    public class MockPublicEmployerRepository : IPagedRepository<EmployerRecord>
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
            //DONE:NastyBug! Page method arguments Page(pageSize, page) where in vice-versa positions as in Page(page, pageSize)! now fixed 
            result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
            result.RowCount = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.PageCount = (int)Math.Ceiling((double)result.RowCount / pageSize);
            return result;
        }

        public string GetSicCodes(string companyNumber)
        {
            return AllEmployers.FirstOrDefault(c => c.CompanyNumber == companyNumber)?.SicCodes;
        }

        PagedResult<EmployerRecord> IPagedRepository<EmployerRecord>.Search(string searchText, int page, int pageSize)
        {
            //var searchResults = PublicSectorOrgs.Messages.List.Where(o => o.Name.ContainsI(searchText));
            int totalRecords;
           
            var result = new PagedResult<EmployerRecord>()
            {
                Results = new List<EmployerRecord>()
                {
                    new EmployerRecord() { Name="2Gether NHS Foundation Trust",                EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="5 Boroughs Partnership NHS Foundation Trust", EmailPatterns = "nhs.uk" },
                    new EmployerRecord() { Name="Abbots Langley Parish Council",               EmailPatterns = "abbotslangley-pc.gov.uk" },
                    new EmployerRecord() { Name="Aberdeen City Council",                       EmailPatterns = "aberdeencityandshire-sdpa.gov.uk" },
                    new EmployerRecord() { Name="Aberdeenshire Council",                       EmailPatterns = "aberdeenshire.gov.uk" },
                    new EmployerRecord() { Name="Aberford &amp; District Parish Council",      EmailPatterns = "aberford-pc.gov.uk" },
                    new EmployerRecord() { Name="Abergavenny Town Council",                    EmailPatterns = "AbergavennyTownCouncil.gov.uk" },
                    new EmployerRecord() { Name="Aberporth Community Council",                 EmailPatterns = "aberporthcommunitycouncil.gov.uk" },
                    new EmployerRecord() { Name="Abertilly and Llanhilleth Community Council", EmailPatterns = "abertilleryandllanhilleth-wcc.gov.uk" },
                    new EmployerRecord() { Name="Aberystwyth Town Council",                    EmailPatterns = "aberystwyth.gov.uk" },
                    new EmployerRecord() { Name="Abingdon Town Council",                       EmailPatterns = "abingdon.gov.uk" },
                    new EmployerRecord() { Name="Academies Enterprise Trust",                  EmailPatterns = "" },
                    new EmployerRecord() { Name="Academy Transformation Trust",                EmailPatterns = "" },
                    new EmployerRecord() { Name="Account NI DFP",                              EmailPatterns = "accountni.gov.uk" },
                    new EmployerRecord() { Name="Accountant in Bankruptcy",                    EmailPatterns = "aib.gov.uk" }
                }
            };

            result.Results = AllEmployers.Where(e => e.Name.ContainsI(searchText)).Page(page, pageSize).ToList();
            result.RowCount = totalRecords = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
            // result.Results = searchResults;
            return result;
        }
    }

    public class MockGovNotify : IGovNotify
    {
        string _Status = "delivered";

        public Notification SendEmail(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            return new Notification()
            {
                status = _Status
            };
        }

        public Notification SendSms(string mobileNumber, string templateId, Dictionary<string, dynamic> personalisation)
        {
            return new Notification()
            {
                status = _Status
            };
        }

        public Notification SendPost(string emailAddress, string templateId, Dictionary<string, dynamic> personalisation)
        {
            return new Notification()
            {
                status = _Status
            };
        }

        public void SetStatus(string status)
        {
            _Status = status;
        }
    }

}
