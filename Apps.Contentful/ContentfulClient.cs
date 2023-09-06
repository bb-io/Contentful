using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace Apps.Contentful
{
    public class ContentfulClient : ContentfulManagementClient
    {
        public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            string spaceId) : base(new HttpClient(), new ContentfulOptions
        {
            ManagementApiKey = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value,
            SpaceId = spaceId
        })
        {
        }
    }
}
