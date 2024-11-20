using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestRequest(string resource, Method method, IEnumerable<AuthenticationCredentialsProvider> creds)
    : BlackBirdRestRequest(resource, method, creds)
{
    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var token = creds.Get("Authorization");
        this.AddHeader(token.KeyName, $"Bearer {token.Value}");
    }
}