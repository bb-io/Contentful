using Apps.Contentful.Api;
using Apps.Contentful.Constants;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Requests.Base;
using Apps.Contentful.Models.Requests.Tags;
using Apps.Contentful.Models.Responses.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Contentful.Actions;

[ActionList]
public class TagActions : ContentfulInvocable
{
    public TagActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    #region Actions

    [Action("List tags", Description = "List all content tags in a space")]
    public async Task<ListTagsResponse> ListTags()
    {
        var response = await Client.GetContentTagsCollection();
        var tags = response.Select(x => new TagEntity(x)).ToArray();

        return new(tags);
    }

    [Action("Create tag", Description = "Create a new content tag")]
    public async Task<TagEntity> CreateTag([ActionParameter] CreateTagRequest input)
    {
        var tag = await Client.CreateContentTag(input.Name, input.TagId, input.IsPublic);
        return new(tag);
    }

    [Action("Get tag", Description = "Get details of a specific tag")]
    public async Task<TagEntity> GetTag([ActionParameter] TagRequest input)
    {
        var tag = await Client.GetContentTag(input.TagId);
        return new(tag);
    }

    [Action("Delete tag", Description = "Delete specific content tag")]
    public async Task DeleteTag([ActionParameter] TagRequest input)
    {
        var tag = await Client.GetContentTag(input.TagId);
        await Client.DeleteContentTag(input.TagId, tag.SystemProperties.Version);
    }

    [Action("Add tag to entry", Description = "Add specific tag to an entry")]
    public async Task AddEntryTag([ActionParameter] TagToEntryInput input)
    {
        var entry = await new ContentfulClient(Creds).GetEntry(input.EntryId);
        
        var tags = entry.Metadata.Tags
            .Select(x => x.Sys.Id)
            .Append(input.TagId)
            .Select(x => new PropertiesRequest()
            {
                Sys = new()
                {
                    Id = x,
                    Type = "Link",
                    LinkType = "Tag"
                }
            })
            .ToArray();

        await ReplaceTags(input.EntryId, entry.SystemProperties.Version, tags);
    }

    [Action("Remove tag from entry", Description = "Remove specific tag from an entry")]
    public async Task RemoveEntryTag([ActionParameter] TagToEntryInput input)
    {
        var entry = await new ContentfulClient(Creds).GetEntry(input.EntryId);
        
        var tags = entry.Metadata.Tags
            .Where(x => x.Sys.Id != input.TagId)
            .Select(x => new PropertiesRequest()
            {
                Sys = new()
                {
                    Id = x.Sys.Id,
                    Type = "Link",
                    LinkType = "Tag"
                }
            })
            .ToArray();

        await ReplaceTags(input.EntryId, entry.SystemProperties.Version, tags);
    }

    #endregion

    #region Utils

    private Task ReplaceTags(string entryId, int? version, PropertiesRequest[] tags)
    {
        var client = new ContentfulRestClient(Creds);

        var endpoint = $"entries/{entryId}";
        var request = new ContentfulRestRequest(endpoint, Method.Patch, Creds)
            .AddHeader("X-Contentful-Version", version ?? default)
            .AddHeader("Content-Type", "application/json-patch+json")
            .WithJsonBody(new PatchRequest<PropertiesRequest[]>[]
            {
                new()
                {
                    Op = "replace",
                    Path = "/metadata/tags",
                    Value = tags
                }
            }, JsonConfig.Settings);

        return client.ExecuteWithErrorHandling(request);
    }

    #endregion
}