using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Constants;
using Newtonsoft.Json;
using OAuth2TestClient;
using Thinktecture.IdentityModel.Client;

namespace OAuth2ApiClient
{
    internal class Program
    {
        private Program p = new Program();

        private static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();
            // Handle Arguments
            var authcoderesponse = args.SkipWhile(a => a != "authresponse").Skip(1).FirstOrDefault();

            if (authcoderesponse != null)
            {
                // request access token
                var accessToken = RequestAccessToken(authcoderesponse);

                // show token in console
                Util.ParseToken(accessToken);

                // validate token
                var tokenValid = Util.ValidateToken(accessToken.AccessToken);
                Console.WriteLine("Token valid: {0}", tokenValid);
            }
            else
            {
                // New Token request
                RequestAuthorizationCode();
            }

            Console.Read();
        }

        private static void RequestAuthorizationCode()
        {
            var authClient = new OAuth2Client(
                new Uri(Constant.TokenEndpointAuthorize),
                Constant.CodeClientId,
                Constant.CodeClientSecret);

            var state = Util.WriteState();
            var requestUrl = authClient.CreateCodeFlowUrl(
                Constant.CodeClientId,
                Constant.Scope,
                Constant.RedirectUriApi,
                state);

            System.Diagnostics.Process.Start(requestUrl);
        }

        private static TokenResponse RequestAccessToken(string authcoderesponse)
        {
            TokenResponse resultToken = null;
            try
            {
                resultToken = Util.GetToken(authcoderesponse, Constant.RedirectUriApi).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() == typeof(HttpRequestException))
                {
                    Console.WriteLine("Exception: {0}", ex.InnerException.Message);
                    Console.Write("Wrong Authorization code?");
                }
                else
                {
                    throw ex;
                }
            }
            return resultToken;
        }
    }
}
