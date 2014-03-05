using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constants
{
    public class Constant
    {
        public const string AuthHost = "localhost:44301";
        public const string ValidateUri = "http://localhost:55226/api/token";
        public const string ClientId = "TestClient";
        public const string Application = "HsrTestApp";

        public const string RedirectUriImplicit = "oauthclient://";
        public const string TokenEndpointAuthorize = "https://" + AuthHost + "/" + Application + "/oauth/authorize";
        public const string TokenEndpoint = "https://" + AuthHost + "/" + Application + "/oauth/token";
        public const string Scope = "read";

        public const string CodeClientId = "HsrCodeClient";
        public const string RedirectUriCode = "oauthclientcode://";
        public const string CodeClientSecret = "secret";

    }
}
