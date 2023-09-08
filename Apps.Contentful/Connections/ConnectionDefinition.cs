using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Contentful.Connections
{
    public class ConnectionDefinition : IConnectionDefinition
    {
        public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>
        {
            new()
            {
                Name = "OAuth2",
                AuthenticationType = ConnectionAuthenticationType.OAuth2,
                ConnectionUsage = ConnectionUsage.Actions,
                ConnectionProperties = new List<ConnectionProperty>
                {
                    new("client_id") { DisplayName = "Client ID" }
                }
            },
            new()
            {
                Name = "Space ID",
                AuthenticationType = ConnectionAuthenticationType.Undefined,
                ConnectionUsage = ConnectionUsage.Actions & ConnectionUsage.Webhooks,
                ConnectionProperties = new List<ConnectionProperty>
                {
                    new("spaceId") { DisplayName = "Space ID" }
                }
            }
        };

        public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
            Dictionary<string, string> values)
        {
            var accessToken = values.First(v => v.Key == "access_token");
            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.Header,
                 "Authorization",
                 accessToken.Value
            );
            var spaceId = values.First(v => v.Key == "spaceId");
            yield return new AuthenticationCredentialsProvider(
                AuthenticationCredentialsRequestLocation.Header,
                spaceId.Key,
                spaceId.Value
            );
        }
    }
}
