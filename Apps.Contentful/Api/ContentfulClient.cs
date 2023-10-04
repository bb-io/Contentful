using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds)
        : base(new HttpClient(), new ContentfulOptions
        {
            ManagementApiKey = creds.First(p => p.KeyName == "Authorization").Value,
            SpaceId = creds.First(p => p.KeyName == "spaceId").Value
        })
    {
    }
}