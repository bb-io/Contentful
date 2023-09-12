using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Actions;

[ActionList]
public class ContentModelActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public ContentModelActions(InvocationContext invocationContext) : base(invocationContext) { }
    
    [Action("List all content models", Description = "List all content models in space.")]
    public async Task<ListAllContentModelsResponse> ListAllContentModels()
    {
        var client = new ContentfulClient(Creds);
        var contentTypes = await client.GetContentTypes();
        return new ListAllContentModelsResponse
        {
            ContentModels = contentTypes.Select(t => new ContentModelDto(t))
        };
    }
}