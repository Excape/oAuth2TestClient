using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Thinktecture.IdentityModel.Client;
using Constants;

namespace OAuth2TestClient
{
    class CodeFlow
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();   
            var p = new CodeFlow();

            // Handle Arguments
            var authcoderesponse = args.SkipWhile(a => a != "authresponse").Skip(1).FirstOrDefault();

            var authclient = new OAuth2Client(
            new Uri(Constant.TokenEndpointAuthorize),
            Constant.CodeClientId,
            Constant.CodeClientSecret);

            if (authcoderesponse != null)
            {
                ReceiveAuthorizationCode(authcoderesponse);
            }
            else
            {
                // Request Authorization code, open URL in browser
                System.Diagnostics.Process.Start(p.CreateAuthUrl(authclient));
            }
            

            Console.Read();
        }

        private static void ReceiveAuthorizationCode(string authcoderesponse)
        {
            var p = new CodeFlow();

            // Get Token
            TokenResponse resultToken = null;
            try
            {
                resultToken = Util.GetToken(authcoderesponse, Constant.RedirectUriCode).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() == typeof (HttpRequestException))
                {
                    Console.WriteLine("Exception: {0}", ex.InnerException.Message);
                    Console.Write("Wrong Authorization code?");
                }
                else
                {
                    throw ex;
                }
            }


            Util.ParseToken(resultToken);

            while (true)
            {
               p.CheckTokenUntilInvalid(resultToken.AccessToken);


                // Refresh token

                var refreshedToken = RefreshToken(resultToken);

                Util.ParseToken(refreshedToken);

                // Check new Token
                Console.WriteLine("New Token valid: {0}", Util.ValidateToken(refreshedToken.AccessToken));
                resultToken = refreshedToken;
            }
           

        }

        private static TokenResponse RefreshToken(TokenResponse resultToken)
        {
            Console.WriteLine("\n--------------------------------");
            Console.WriteLine("Refresh Token\n{0}\n", resultToken.RefreshToken);

            TokenResponse refreshedToken = null;
            try
            {
                refreshedToken = Util.RefreshToken(resultToken).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() == typeof (HttpRequestException))
                {
                    Console.WriteLine("Exception: {0}", ex.InnerException.Message);
                    Console.Write("Refresh Token expired?");
                }
                else
                {
                    throw ex;
                }
            }
            return refreshedToken;
        }

        private string CreateAuthUrl(OAuth2Client authClient)
        {
            var requestUrl = authClient.CreateCodeFlowUrl(
                Constant.CodeClientId,
                Constant.Scope,
                Constant.RedirectUriCode);

            Console.WriteLine("Request Authorization code:\n" + requestUrl);

            return requestUrl;
        }

        private void CheckTokenUntilInvalid(string accessToken)
        {
            bool tokenIsValid;
            int count = 0;
            Console.WriteLine("Wait until token is expired:\n");

            do
            {
                Thread.Sleep(10000);
                count += 10;
                tokenIsValid = Util.ValidateToken(accessToken);
                Console.WriteLine("{0} seconds - Token valid: {1}", count, tokenIsValid);
            } while (tokenIsValid);
        }

        
    }
}
