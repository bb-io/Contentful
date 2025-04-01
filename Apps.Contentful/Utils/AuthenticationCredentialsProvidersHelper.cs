using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;

namespace Apps.Contentful.Utils;

public static class AuthenticationCredentialsProvidersHelper
{
    public static string GetSpaceId(this IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var spaceIdProvider = authenticationCredentialsProviders.Get(CredNames.SpaceId);
        if(string.IsNullOrEmpty(spaceIdProvider.Value))
        {
            throw new Exception($"Missing {CredNames.SpaceId} authentication credentials provider.");
        }

        return spaceIdProvider.Value;
    }
}
