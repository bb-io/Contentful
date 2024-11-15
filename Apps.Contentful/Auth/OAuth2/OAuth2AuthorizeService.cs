using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Contentful.Auth.OAuth2;

public class OAuth2AuthorizeService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        var oauthUrl = GetOAuthUrl();
        var parameters = new Dictionary<string, string>
        {
            { CredNames.ClientId, values["client_id"] },
            { "redirect_uri", InvocationContext.UriInfo.ImplicitGrantRedirectUri.ToString() },
            { "response_type", "token" },
            { "scope", ApplicationConstants.Scope },
            { "state", values["state"] }
        };
        
        return QueryHelpers.AddQueryString(oauthUrl, parameters);
    }

    private string GetOAuthUrl()
    {
        var baseUrl = InvocationContext.AuthenticationCredentialsProviders
            .Get(CredNames.BaseUrl).Value;
        
        if(baseUrl.Contains("eu"))
        {
            return "https://be.eu.contentful.com/oauth/authorize";
        }
        
        return "https://be.contentful.com/oauth/authorize";
    }
}