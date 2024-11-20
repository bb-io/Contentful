using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Apps.Contentful.Auth.OAuth2;

public class OAuth2AuthorizeService(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        var bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth"; //.Replace("https://bridge.blackbird.io","https://9cc8-176-36-119-50.ngrok-free.app")
        var oauthUrl = GetOAuthUrl(values);
        var parameters = new Dictionary<string, string>
        {
            { CredNames.ClientId, values["client_id"] },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/ImplicitGrant" }, //.Replace("https://bridge.blackbird.io", "https://9cc8-176-36-119-50.ngrok-free.app")
            { "actual_redirect_uri", InvocationContext.UriInfo.ImplicitGrantRedirectUri.ToString() },
            { "authorization_url", oauthUrl},            
            { "response_type", "token" },
            { "scope", ApplicationConstants.Scope },
            { "state", values["state"] }
        };
        
        return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters);
    }

    private string GetOAuthUrl(Dictionary<string, string> values)
    {
        if(!values.TryGetValue(CredNames.BaseUrl, out var baseUrl))
        {
            throw new InvalidOperationException($"Base URL is not set. Values: {JsonConvert.SerializeObject(values)}");
        }
        
        return baseUrl.Contains("eu") 
            ? "https://be.eu.contentful.com/oauth/authorize" 
            : "https://be.contentful.com/oauth/authorize";
    }
}