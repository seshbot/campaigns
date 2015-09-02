using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Campaigns.Models.Sessions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin;
using Owin;
using System.Reflection;
using System.Web.Mvc;

[assembly: OwinStartup(typeof(Campaigns.Startup))]
namespace Campaigns
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //
            // AutoFac
            //

            var builder = new ContainerBuilder();

            // STANDARD MVC SETUP:

            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register your SignalR hubs.
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            builder.RegisterModule(new CampaignsModule());

            // Run other optional steps, like registering model binders,
            // web abstractions, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);

            // OWIN MVC SETUP:

            //// Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            //app.UseAutofacMiddleware(container);
            //app.UseAutofacMvc();

            //GlobalHost.DependencyResolver.Register(
            //    typeof(ChatHub),
            //    () => new ChatHub(new ChatMessageRepository()));

            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = true;
            app.MapSignalR("/signalr", hubConfiguration);

            //
            // OWIN
            //

            //app.MapSignalR();
            ConfigureAuth(app);
        }
    }

    public class CampaignsModule : Autofac.Module
    {
        private static IHubConnectionContext<dynamic> ResolveHubClients(IComponentContext ctx)
        {
            return GlobalHost.DependencyResolver
                .Resolve<IConnectionManager>()
                .GetHubContext<SessionHub>()
                .Clients;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SessionService>()
                .WithParameter(ResolvedParameter.ForNamed<IHubConnectionContext<dynamic>>("SessionHubClients"))
                .As<ISessionService>()
                .SingleInstance();

            builder.Register(ResolveHubClients).Named<IHubConnectionContext<dynamic>>("SessionHubClients"); ;

            base.Load(builder);
        }
    }
}
