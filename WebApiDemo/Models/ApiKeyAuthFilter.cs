using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace WebApiDemo.Models
{
    //REF: https://docs.microsoft.com/zh-tw/aspnet/web-api/overview/security/authentication-filters
    //REF: https://dotnetcodr.com/2015/07/23/web-api-2-security-extensibility-points-part-2-custom-authentication-filter/
    /// <inheritdoc />
    public class ApiKeyAuthAttribute : Attribute, IAuthenticationFilter
    {
        /// <inheritdoc />
        public bool AllowMultiple => false;

        private const string API_KEY_HEADER_NAME = "X-Api-Key";

        private ClaimsPrincipal CreateClaimPrinciple(string apiClientId)
        {
            //TODO 實際應用時由資料庫或設定檔檢核API Key及限定IP
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, apiClientId));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "MyApiAuth"));
        }

        //REF: http://www.herlitz.nu/2013/06/27/getting-the-client-ip-via-asp-net-web-api/
        private string GetIP(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }


        /// <inheritdoc />
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var ip = GetIP(request);
            ClaimsPrincipal principle = null;
            if (request.Headers.Contains(API_KEY_HEADER_NAME))
            {
                var xApiKey = request.Headers.GetValues(API_KEY_HEADER_NAME).FirstOrDefault();
                var apiClientId = AccessControlManager.GetApiClientId(xApiKey, ip);
                if (!string.IsNullOrEmpty(apiClientId))
                {
                    principle  = CreateClaimPrinciple(apiClientId);
                    context.Principal = principle;
                }
            }
            if (principle == null)
                context.ErrorResult = new AuthenticationFailureResult("Invalid API key.", request);
        }

        /// <inheritdoc />
        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return;
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
        {
            ReasonPhrase = reasonPhrase;
            Request = request;
        }

        public string ReasonPhrase { get; private set; }

        public HttpRequestMessage Request { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            response.ReasonPhrase = ReasonPhrase;
            return response;
        }
    }

}