using Apps.Contentful.Constants;
using Apps.Contentful.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Contentful.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
    {
        new()
        {
            Name = "OAuth2",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredNames.ClientId) { DisplayName = "Client ID" },
                new(CredNames.BaseUrl) 
                { 
                    DisplayName = "Base URL", 
                    Description = "The base URL of the Contentful API. " +
                                  "Example: https://api.contentful.com or https://api.eu.contentful.com",
                    DataItems = [new ("https://api.contentful.com", "(Default) api.contentful.com"), 
                        new ("https://api.eu.contentful.com", "(Europe) api.eu.contentful.com")]
                },
                new(CredNames.SpaceId) { DisplayName = "Space ID" }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var accessToken = values.First(v => v.Key == "access_token");
        yield return new AuthenticationCredentialsProvider(
            "Authorization",
            accessToken.Value
        );
        var spaceId = values.First(v => v.Key == "spaceId");
        yield return new AuthenticationCredentialsProvider(
            spaceId.Key,
            spaceId.Value
        );
        var baseUrl = values.First(v => v.Key == CredNames.BaseUrl);
        yield return new AuthenticationCredentialsProvider(
            baseUrl.Key,
            UrlHelper.FormatUrl(baseUrl.Value)
        );
    }
}