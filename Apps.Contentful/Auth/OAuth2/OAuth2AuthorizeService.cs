using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Contentful.Auth.OAuth2
{
    public class OAuth2AuthorizeService : IOAuth2AuthorizeService
    {
        public string GetAuthorizationUrl(Dictionary<string, string> values)
        {
            const string oauthUrl = "https://be.contentful.com/oauth/authorize";
            var parameters = new Dictionary<string, string>
            {
                { "client_id", values["client_id"] },
                { "redirect_uri", ApplicationConstants.RedirectUri },
                { "response_type", "token" },
                { "scope", ApplicationConstants.Scope },
                { "state", values["state"] }
            };
            return QueryHelpers.AddQueryString(oauthUrl, parameters);
        }
    }
}
