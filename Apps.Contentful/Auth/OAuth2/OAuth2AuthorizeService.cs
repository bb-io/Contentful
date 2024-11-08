using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Contentful.Auth.OAuth2;

public class OAuth2AuthorizeService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {        
        var bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
        const string oauthUrl = "https://be.contentful.com/oauth/authorize";
        var parameters = new Dictionary<string, string>
        {
            { CredNames.ClientId, values["client_id"] },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "actual_redirect_uri", InvocationContext.UriInfo.ImplicitGrantRedirectUri.ToString() },
            { "authorization_url", oauthUrl},            { "response_type", "token" },
            { "scope", ApplicationConstants.Scope },
            { "state", values["state"] }
        };
        
        return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters);
    }
}