using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(campaigns.Startup))]
namespace campaigns
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
