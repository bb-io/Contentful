using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace Apps.Contentful
{
    public class ContentfulClient : ContentfulManagementClient
    {
        public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
            : base(new HttpClient(), new ContentfulOptions
        {
            ManagementApiKey = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value,
            SpaceId = authenticationCredentialsProviders.First(p => p.KeyName == "spaceId").Value
        })
        {
        }
    }
}
