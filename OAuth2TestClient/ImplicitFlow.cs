using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Tokens.Http;
using Constants;

namespace OAuth2TestClient
{
    class ImplicitFlow
    {

        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            var p = new ImplicitFlow();

            // Handle Arguments
            var tokenresponse = args.SkipWhile(a => a != "tokenresponse").Skip(1).FirstOrDefault();

            if (tokenresponse != null)
            {
                var token = p.ParseToken(tokenresponse);

                Util.ValidateToken(token);
            }
            else
            {
                var client = new OAuth2Client(
                new Uri(Constant.TokenEndpointAuthorize));

                // Open Browser
                System.Diagnostics.Process.Start(p.CreateRequestUrl(client));
            }

            Console.Read();
        }

        

        private string CreateRequestUrl(OAuth2Client oAuthClient)
        {
            var requestUrl = oAuthClient.CreateImplicitFlowUrl(
                Constant.ClientId,
                Constant.Scope,
                Constant.RedirectUriImplicit);

            Console.WriteLine("Request Token:\n" + requestUrl);
            

            return requestUrl;
        }

        private string ParseToken(string responseUri)
        {
            var tokenQuery = new Uri(responseUri).Fragment;
            const string regex = @"^#access_token=(.+)&token_type=(.+)&expires_in=(.+)$";

            Console.WriteLine("Parse Token:\n{0}\n", responseUri);

            var m = Regex.Match(tokenQuery, regex);
            if (m.Success)
            {
                Console.WriteLine("Access Token:\n{0}\n", m.Groups[1].Value);
                Console.WriteLine("Token Type:\n{0}\n", m.Groups[2].Value);

                var now = DateTime.Now;
                var expireDate = now.AddSeconds(Convert.ToDouble(m.Groups[3].Value));
                Console.WriteLine("Expires:\n{0} ({1}s)\n", expireDate, m.Groups[3].Value);

                return m.Groups[1].Value;
            }
            else
            {
                Console.WriteLine("Wrong Token format!");
                return null;
            }
        }
    }
}
