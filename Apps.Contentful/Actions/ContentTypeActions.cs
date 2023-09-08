using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Actions;

[ActionList]
public class ContentTypeActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public ContentTypeActions(InvocationContext invocationContext) : base(invocationContext) { }
    
    [Action("Get all content types", Description = "Get all content types in space.")]
    public async Task<GetAllContentTypesResponse> GetAllContentTypes()
    {
        var client = new ContentfulClient(Creds);
        var contentTypes = await client.GetContentTypes();
        var contentTypeDtos = contentTypes.Select(t => new ContentTypeDto { Name = t.Name }).ToList();
        return new GetAllContentTypesResponse
        {
            ContentTypes = contentTypeDtos
        };
    }
}