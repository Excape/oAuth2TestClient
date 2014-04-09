using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Thinktecture.IdentityModel.WebApi.Authentication.Handler;

namespace TestRessourceServer.Controllers
{
    public class TokenController : ApiController
    {
        public IHttpActionResult Get(HttpRequestMessage request)
        {

            var authN = new HttpAuthentication(WebApiConfig.configuration);

            // Code to minimize time after token expiration when token is still successfully validated. Just for test purposes!
            authN.Configuration.Mappings.First().TokenHandler.Configuration.MaxClockSkew = TimeSpan.FromSeconds(3);
            try
            {
                ClaimsPrincipal principal = authN.Authenticate(request);
                if (principal.Identity.IsAuthenticated == false)
                {
                    return StatusCode(HttpStatusCode.Forbidden);
                }
            } catch (SecurityTokenValidationException ex)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            return Ok();
        }
    }
}
