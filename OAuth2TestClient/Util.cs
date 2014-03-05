using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Net;
using System.Text;
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

            var debugrefresh = "f65571d6e4394027a337c09c37f7d06f";

            //return await client.RequestRefreshTokenAsync(debugrefresh);
            return await client.RequestRefreshTokenAsync(token.RefreshToken);
        }
    }
}
