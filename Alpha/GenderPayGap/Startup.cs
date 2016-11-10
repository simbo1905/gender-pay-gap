using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GenderPayGap.Startup))]
namespace GenderPayGap
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
