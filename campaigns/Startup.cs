using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Campaigns.Core.Data;
using Campaigns.Model.Data;
using Campaigns.Models.Sessions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin;
using Owin;
using Services.Rules;
using System;
using System.Reflection;
using System.Web.Http;
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

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // Register MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            // Register Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register the Autofac filter provider.
            //builder.RegisterWebApiFilterProvider(config);

            // Register your SignalR hubs.
            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            builder.RegisterModule(new CampaignsModule());

            // Run other optional steps, like registering model binders,
            // web abstractions, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

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

    public class CampaignsRepositories : IDisposable
    {
        private CampaignsDbContext _dbContext = new CampaignsDbContext();

        public IEntityRepository<Campaigns.Model.Attribute> Attributes { get; private set; }
        public IEntityRepository<Campaigns.Model.AttributeContribution> AttributeContributions { get; set; }
        public IEntityRepository<Campaigns.Model.CharacterSheet> CharacterSheets { get; set; }
        public IEntityRepository<Campaigns.Model.Character> Characters { get; set; }

        public CampaignsRepositories()
        {
            Attributes = new EFEntityRepository<Campaigns.Model.Attribute>(_dbContext, _dbContext.Attributes);
            AttributeContributions = new EFEntityRepository<Campaigns.Model.AttributeContribution>(_dbContext, _dbContext.AttributeContributions);
            CharacterSheets = new EFEntityRepository<Campaigns.Model.CharacterSheet>(_dbContext, _dbContext.CharacterSheets);
            Characters = new EFEntityRepository<Campaigns.Model.Character>(_dbContext, _dbContext.Characters);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
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
            //
            // Data layer
            //

            builder.RegisterType<CampaignsRepositories>()
                .InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<CampaignsRepositories>().Attributes)
                .As<IEntityStore<Campaigns.Model.Attribute>>()
                .As<IEntityRepository<Campaigns.Model.Attribute>>();
            builder.Register(c => c.Resolve<CampaignsRepositories>().AttributeContributions)
                .As<IEntityStore<Campaigns.Model.AttributeContribution>>()
                .As<IEntityRepository<Campaigns.Model.AttributeContribution>>();
            builder.Register(c => c.Resolve<CampaignsRepositories>().CharacterSheets)
                .As<IEntityStore<Campaigns.Model.CharacterSheet>>()
                .As<IEntityRepository<Campaigns.Model.CharacterSheet>>();
            builder.Register(c => c.Resolve<CampaignsRepositories>().Characters)
                .As<IEntityStore<Campaigns.Model.Character>>()
                .As<IEntityRepository<Campaigns.Model.Character>>();

            //
            // Service Layer
            //

            builder.RegisterType<RulesService>().As<IRulesService>();

            builder.RegisterType<SessionService>()
                .WithParameter(ResolvedParameter.ForNamed<IHubConnectionContext<dynamic>>("SessionHubClients"))
                .As<ISessionService>()
                .SingleInstance();

            //
            // Application Layer
            //

            // Session Hub
            builder.Register(ResolveHubClients).Named<IHubConnectionContext<dynamic>>("SessionHubClients"); ;

            base.Load(builder);
        }
    }
}
