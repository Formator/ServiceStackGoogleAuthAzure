using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Funq;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Authentication.OAuth2;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.Logging.NLogger;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;


namespace GoogleAuthAzure
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("GoogleAuthAzure", typeof(AppHost).Assembly)
        {

        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            LogManager.LogFactory = new NLogFactory();
            SetConfig(new HostConfig
            {
                HandlerFactoryPath = "api",
            });
            //Config examples
            //this.Plugins.Add(new PostmanFeature());
            //this.Plugins.Add(new CorsFeature());

            //Set MVC to use the same Funq IOC as ServiceStack
            ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));

            //Register all Authentication methods you want to enable for this web app.            
            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                {
                    new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
                    new BasicAuthProvider(), //Sign-in with HTTP Basic Auth
                    new GoogleOAuth2Provider(AppSettings), //Sign-in with Google OAuth2 Provider
                    new FacebookAuthProvider(AppSettings)
                })
            {
                HtmlRedirect = "/Home/Result"
            });
            Plugins.Add(new RegistrationFeature());

            //Create a DB Factory configured to access the UserAuth PostgreSQL DB
            container.Register<IDbConnectionFactory>(c =>
                new OrmLiteConnectionFactory(AppSettings.GetString("AppDb"), SqlServerDialect.Provider)
                {
                    ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
                });

            //Store User Data into the referenced SqlServer database
            container.Register<IUserAuthRepository>(c =>
                new OrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>()) //Use OrmLite DB Connection to persist the UserAuth and AuthProvider info
                {
                    UseDistinctRoleTables = AppSettings.Get("UseDistinctRoleTables", true),
                });
            var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>(); //If using and RDBMS to persist UserAuth, we must create required tables

            if (AppSettings.Get("RecreateAuthTables", false))
                authRepo.DropAndReCreateTables(); // Drop and re-create all Auth and registration tables
            else
                authRepo.InitSchema();   // Create only the missing tables

            GlobalResponseFilters.Add((req, res, dto) =>
            {
                if (!req.PathInfo.EndsWith("/auth/GoogleOAuth", true, CultureInfo.InvariantCulture))
                {
                    return;
                }
                var httpResult = dto as HttpResult;
                if (httpResult != null && httpResult.Headers.ContainsKey("Content-Length"))
                {
                    httpResult.Headers.Remove("Content-Length");
                }
            });
        }
    }
}