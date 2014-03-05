using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
                resultToken = p.GetToken(authcoderesponse).Result;
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


            p.ParseToken(resultToken);

            while (true)
            {
               p.CheckTokenUntilInvalid(resultToken.AccessToken);


                // Refresh token

                var refreshedToken = RefreshToken(resultToken);

                p.ParseToken(refreshedToken);

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

        private async Task<TokenResponse> GetToken(string authCodeRaw)
        {
            // Parse Authcode
            var authParam = new Uri(authCodeRaw).Query;
            const string regex = @"^\?code=(.+)$";
            var m = Regex.Match(authParam, regex);
            if (m.Success)
            {
                Console.WriteLine("Request token with Authorization code\n{0}\n", m.Groups[1].Value);
                return await SendTokenRequest(m.Groups[1].Value);
            }

            throw new Exception("Wrong AuthCode Format!");

        }

        static async Task<TokenResponse> SendTokenRequest(string authCode)
        {
            var tokenclient = new OAuth2Client(
                new Uri(Constant.TokenEndpoint),
                Constant.CodeClientId,
                Constant.CodeClientSecret);

            return await tokenclient.RequestAuthorizationCodeAsync(authCode, Constant.RedirectUriCode);
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

        private void ParseToken(TokenResponse token)
        {
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadToken(token.AccessToken) as JwtSecurityToken;

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Access Token:\n{0}\n\n", token.AccessToken); 
            Console.WriteLine("Refresh Token:\n{0}\n\n", token.RefreshToken);
            Console.WriteLine("Token Type:\n{0}\n", token.TokenType);

            Console.WriteLine("Issuer:\n{0}\n", decodedToken.Issuer);
            Console.WriteLine("Audience:\n{0}\n", decodedToken.Audience);
            Console.WriteLine("Valid:\n{0} - {1}\n", decodedToken.ValidFrom, decodedToken.ValidTo);
            Console.WriteLine("User:\n{0}\n", decodedToken.Subject);
            Console.WriteLine("Scopes:");
            foreach (var cl in decodedToken.Claims.Where(c => c.Type == "scope"))
            {
                Console.Write("{0} ", cl.Value); 
            }
            Console.WriteLine("\n\n--------------------------------");
        }
    }
}
