using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
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

            // retrieve certificate for x509-signed tokens
            //var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //store.Open(OpenFlags.ReadOnly);
            //var clientCert =
            //    store.Certificates.OfType<X509Certificate2>()
            //        .FirstOrDefault(c => c.Subject == "CN=CleanerClient");
            //store.Close();

            
            Configuration.AddJsonWebToken(
                issuer: "AS",
                audience: "HsrTestApp",
                //signingCertificate: clientCert,
                signingKey: "i4SpI3zdts0yIHhfbBIeR4VuG1MJCfM1wcUZ2LVPFWA=", //use symmetric key instead of signing cert
                scheme: "bearer");

            // Debug config to disable chain validation for self-signed certs (not needed if peer validation is used)

            //var authTokenHandleConfig = Configuration.Mappings.First().TokenHandler.First().Configuration;
            //authTokenHandleConfig.RevocationMode = X509RevocationMode.NoCheck;
            //authTokenHandleConfig.CertificateValidationMode = X509CertificateValidationMode.None;
        }
    }
}
