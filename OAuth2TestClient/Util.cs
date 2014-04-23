using System;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Constants;
using Thinktecture.IdentityModel.Client;

namespace OAuth2TestClient
{
    public static class Util
    {
        public static bool ValidateToken(string token)
        {
            // According to RFC 6750 Section 2.1, the bearer token needs to be Base64 encoded
            // However, Thinktecture IdentityModel Web API v1.1.0 doesn't convert back from Base64
            // therefore, we're sending the actual token "as is" in the authentication header

            //var b64Token = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));

            var request = (HttpWebRequest) WebRequest.Create(Constant.ValidateUri);
            //request.Headers.Add("Authorization", "Bearer " + b64Token);
            request.Headers.Add("Authorization", "Bearer " + token);
            try
            {
                var response = (HttpWebResponse) request.GetResponse();

                return (int)response.StatusCode == 200;
            }
            catch (WebException ex)
            {
                var statusCode = (int) ((HttpWebResponse) ex.Response).StatusCode;

                if (statusCode != 403) // Forbidden access code, sendt by the ressource server when the token expired
                {
                    throw new Exception("Token validation failed", ex);
                }
                return false;
            }
        }

        public async static Task<TokenResponse> RefreshToken(TokenResponse token)
        {
            var client = new OAuth2Client(
                 new Uri(Constant.TokenEndpoint),
                 Constant.CodeClientId,
                 Constant.CodeClientSecret);

            return await client.RequestRefreshTokenAsync(token.RefreshToken);
        }

        public static async Task<TokenResponse> GetToken(string authCodeRaw, string redirectUri)
        {
            // Parse Authcode
            var authParam = new Uri(authCodeRaw).Query;
            const string regex = @"^\?code=(.{32})(&state=)?(.*)$";
            var m = Regex.Match(authParam, regex);
            if (m.Success)
            {
                if (m.Groups[3].Value != String.Empty)
                {
                    if (m.Groups[3].Value == ReadState())
                    {
                        Console.WriteLine("State matching:\n{0}\n", m.Groups[3].Value);
                    }
                    else
                    {
                        throw new Exception("State not matching!");
                    }
                    
                }


                Console.WriteLine("Request token with Authorization code\n{0}\n", m.Groups[1].Value);
                return await SendTokenRequest(m.Groups[1].Value, redirectUri);
            }

            throw new Exception("Wrong AuthCode Format!");

        }

        private static async Task<TokenResponse> SendTokenRequest(string authCode, string redirectUri)
        {
            var tokenclient = new OAuth2Client(
                new Uri(Constant.TokenEndpoint),
                Constant.CodeClientId,
                Constant.CodeClientSecret);

            return await tokenclient.RequestAuthorizationCodeAsync(authCode, redirectUri);
        }

        public static void ParseToken(TokenResponse token)
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

        public static string WriteState()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "state";
            var state = Convert.ToString(Guid.NewGuid());
            var writer = new StreamWriter(path, false);
            writer.WriteLine(state);
            writer.Close();

            return state;
        }

        public static string ReadState()
        {
            try
            {
                string state;
                var path = AppDomain.CurrentDomain.BaseDirectory + "state";
                using (var sr = new StreamReader(path))
                {
                    state = sr.ReadLine();
                }
                return state;
            }
            catch (Exception ex)
            {
                throw new Exception("state file could not be read!", ex);
            }
        }
    }
}
