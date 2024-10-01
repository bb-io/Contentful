using Apps.Contentful.Api;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Contentful.Core.Models;

namespace Apps.Contentful.Actions;

[ActionList]
public class ContentTypeActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;
    
    [Action("Search content types", Description = "Search for all content types available in space")]
    public async Task<SearchContentTypesResponse> SearchContentTypesAsync([ActionParameter] ContentTypeRequest request)
    {
        var client = new ContentfulClient(Creds, request.Environment);
        var spaceId = Creds.First(p => p.KeyName == "spaceId").Value;
        var contentTypes = await client.GetContentTypes(spaceId, CancellationToken.None);

        var enumerable = contentTypes as ContentType[] ?? contentTypes.ToArray();
        return new SearchContentTypesResponse
        {
            ContentTypes = enumerable?.Select(p => new ContentTypeResponse
            {
                Id = p.SystemProperties.Id,
                Name = p.Name,
                Description = p.Description,
                DisplayField = p.DisplayField,
                Fields = p.Fields.Select(f => new FieldResponse
                {
                    Id = f.Id,
                    Name = f.Name,
                    Type = f.Type,
                    Localized = f.Localized,
                    Required = f.Required,
                    Disabled = f.Disabled,
                    Omitted = f.Omitted
                }).ToList(),
                Locale = p.SystemProperties.Locale
            })?.ToList() ?? new(),
            TotalCount = enumerable?.Length ?? 0
        };
    }
}