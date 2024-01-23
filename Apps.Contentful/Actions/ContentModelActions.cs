using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Actions;

[ActionList]
public class ContentModelActions : ContentfulInvocable
{
    public ContentModelActions(InvocationContext invocationContext) : base(invocationContext) { }
    
    [Action("List all content models", Description = "List all content models in space.")]
    public async Task<ListAllContentModelsResponse> ListAllContentModels([ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, environment.Environment);
        var contentTypes = await client.GetContentTypes();
        
        return new()
        {
            ContentModels = contentTypes.Select(t => new ContentModelDto(t))
        };
    }    
    
    [Action("Get content model", Description = "Get details of a specific content model")]
    public async Task<ContentModelDto> GetContentModel([ActionParameter] ContentModelIdentifier input)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, input.Environment);
        
        var contentType = await client.GetContentType(input.ContentModelId);
        return new(contentType);
    }
}