using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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

                // Request ressources from ressource server
                RequestRessources(accessToken);
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

        private static void RequestRessources(TokenResponse accessToken)
        {
            var regex = @"^\d{1,3}$";
            var validInput = false;
            string userinput;
            do
            {
                Console.Write("ID des Datensatzes auf Ressource Server angeben (0 für alle): ");
                userinput = Console.ReadLine();
                if (userinput != null)
                {
                    validInput = Regex.Match(userinput, regex).Success;
                }
            } while (!validInput);

            var requestId = Convert.ToInt32(userinput);

            var requestPath = requestId == 0 ? Constant.RessourceUrl : Constant.RessourceUrl + requestId ;

            var tokenValid = SendRessourceRequest(accessToken, requestPath);

            if (tokenValid == false)
            {
                Console.WriteLine("Token invalid, trying to refresh access token...");
                var refreshedToken = Util.RefreshToken(accessToken).Result;
                var newTokenValid = SendRessourceRequest(refreshedToken, requestPath);

                if (newTokenValid == false)
                {
                    Console.WriteLine("Refresh token expired or access token invalid");
                }
            }


        }

        private static bool SendRessourceRequest(TokenResponse accessToken, string requestPath)
        {
            var request = (HttpWebRequest) WebRequest.Create(requestPath);

            request.Headers.Add("Authorization", "Bearer " + accessToken.AccessToken);
            request.ContentType = "application/xml";

            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                ReadXml(response);
                return true;
            }
            catch (WebException ex)
            {
                var statusCode = (int) ((HttpWebResponse) ex.Response).StatusCode;

                if (statusCode != 403) // Forbidden access code, sendt by the ressource server when the token expired
                {
                    throw new Exception("Token validation failed", ex);
                }
                // token invalid (403)
                return false;
            }
        }

        private static void ReadXml(HttpWebResponse response)
        {
            var rs = response.GetResponseStream();
            if (rs != null)
            {
                var movieList = new List<Movie>();

                var readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreWhitespace = true;
                using (var reader = XmlReader.Create(rs, readerSettings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Movie")
                        {
                            var movie = new Movie();

                            while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Movie") &&
                                   reader.Read())
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        switch (reader.Name)
                                        {
                                            case "MovieId":
                                                movie.MovieId = Convert.ToInt32(reader.ReadString());
                                                break;
                                            case "Title":
                                                movie.Title = reader.ReadString();
                                                break;
                                            case "DirectorId":
                                                movie.DirectorId = Convert.ToInt32(reader.ReadString());
                                                break;
                                        }
                                    break;
                                }
                            }
                            movieList.Add(movie);
                        }
                    }
                }

                DisplayXml(movieList);
            }
        }

        private static void DisplayXml(List<Movie> movieList)
        {
            if (movieList.Count > 0)
            {
                Console.WriteLine("---------------------------");
                Console.WriteLine("\nID\tTitle\t\t\tDirector");
            }
            foreach (var movie in movieList)
            {
                Console.WriteLine("{0}\t{1}\t\t\t{2}", movie.MovieId, movie.Title, movie.DirectorId);
            }
            
        }
    }
}
