using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using TestRessourceServer.Models;
using Thinktecture.IdentityModel.WebApi.Authentication.Handler; 

namespace TestRessourceServer.Controllers
{
    public class RessourceController : ApiController
    {
        private OAuth2TestDbEntities db = new OAuth2TestDbEntities();
        

        // GET api/Ressource
        [ResponseType(typeof (Movie))]
        public IHttpActionResult GetMovie(HttpRequestMessage request)
        {
            if (AuthorizeRequest(request))
            {
                return Ok(db.Movie.ToList());
            }

            return StatusCode(HttpStatusCode.Forbidden);
        }


        // GET api/Ressource/5
        [ResponseType(typeof(Movie))]
        public IHttpActionResult GetMovie(int id, HttpRequestMessage request)
        {
            if (AuthorizeRequest(request))
            {
                var movie = db.Movie.Find(id);
                if (movie == null)
                {
                    return NotFound();
                }

                return Ok(movie);
            }

            return StatusCode(HttpStatusCode.Forbidden);
        }

        private static bool AuthorizeRequest(HttpRequestMessage request)
        {
            return true;
            //var authN = new HttpAuthentication(WebApiConfig.configuration);

            //// Code to minimize time after token expiration when token is still successfully validated. Just for test purposes!
            //authN.Configuration.Mappings.First().TokenHandler.Configuration.MaxClockSkew = TimeSpan.FromSeconds(3);

            //ClaimsPrincipal principal;
            //try
            //{
            //    principal = authN.Authenticate(request);
            //}
            //catch (SecurityTokenValidationException ex)
            //{
            //    return false;
            //}
            //return principal.Identity.IsAuthenticated;

        }
    }
}