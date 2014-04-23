using System.Web.Http;
using Thinktecture.IdentityModel.WebApi.Authentication.Handler;

namespace TestRessourceServer
{
    public static class WebApiConfig
    {
        public static AuthenticationConfiguration Configuration = new AuthenticationConfiguration();

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            Configuration.AddJsonWebToken(
                issuer: "AS",
                audience: "HsrTestApp",
                signingKey: "i4SpI3zdts0yIHhfbBIeR4VuG1MJCfM1wcUZ2LVPFWA=",
                scheme: "bearer");
        }
    }
}
