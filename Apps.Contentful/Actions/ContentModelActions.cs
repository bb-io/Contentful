using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;
using Apps.Contentful.Dtos.Raw;
using Apps.Contentful.Utils;

namespace Apps.Contentful.Actions;

[ActionList]
public class ContentModelActions(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    [Action("Search all content models", Description = "Search for all content models in space.")]
    public async Task<ListAllContentModelsResponse> ListAllContentModels(
        [ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulRestClient(Creds, environment.Environment);
        var request = new ContentfulRestRequest("content_types", Method.Get, Creds);
        var contentTypes = await client.Paginate<ContentTypeItem>(request);

        return new()
        {
            ContentModels = contentTypes.Select(t => new ContentModelDto(t))
        };
    }

    [Action("Get content model", Description = "Get details of a specific content model")]
    public async Task<ContentModelDto> GetContentModel([ActionParameter] ContentModelIdentifier input)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, input.Environment);

        var contentType = await ExceptionWrapper.ExecuteWithErrorHandling(async () => 
            await client.GetContentType(input.ContentModelId));
        return new(contentType);
    }
}