using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Campaigns.Startup))]
namespace Campaigns
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
