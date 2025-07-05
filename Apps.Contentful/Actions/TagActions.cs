using Apps.Contentful.Api;
using Apps.Contentful.Constants;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests.Base;
using Apps.Contentful.Models.Requests.Tags;
using Apps.Contentful.Models.Responses.Tags;
using Apps.Contentful.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Contentful.Actions;

[ActionList("Tags")]
public class TagActions(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    #region Actions

    [Action("Search tags", Description = "Search for all content tags in a space")]
    public async Task<ListTagsResponse> ListTags([ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var response = await client.ExecuteWithErrorHandling(async () => 
            await client.GetContentTagsCollection());
        
        var tags = response.Select(x => new TagEntity(x)).ToArray();
        return new(tags);
    }

    [Action("Create tag", Description = "Create a new content tag")]
    public async Task<TagEntity> CreateTag(
        [ActionParameter] CreateTagRequest input,
        [ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var tag = await client.ExecuteWithErrorHandling(async () => 
            await client.CreateContentTag(input.Name, input.TagId, input.IsPublic));
        return new(tag);
    }

    [Action("Get tag", Description = "Get details of a specific tag")]
    public async Task<TagEntity> GetTag([ActionParameter] TagRequest input)
    {
        var client = new ContentfulClient(Creds, input.Environment);
        var tag = await client.ExecuteWithErrorHandling(async () => 
            await client.GetContentTag(input.TagId));
        
        return new(tag);
    }

    [Action("Delete tag", Description = "Delete specific content tag")]
    public async Task DeleteTag([ActionParameter] TagRequest input)
    {
        var client = new ContentfulClient(Creds, input.Environment);
        var tag = await client.ExecuteWithErrorHandling(async () => 
            await client.GetContentTag(input.TagId));
        
        await client.ExecuteWithErrorHandling(async () => 
            await client.DeleteContentTag(input.TagId, tag.SystemProperties.Version));
    }

    [Action("Add tag to entry", Description = "Add specific tag to an entry")]
    public async Task AddEntryTag([ActionParameter] EntryTagIdentifier input)
    {
        if (string.IsNullOrEmpty(input.TagId))
        {
            throw new PluginMisconfigurationException("Tag ID is null or empty. Please input a valid ID");
        }
        
        if (string.IsNullOrEmpty(input.EntryId))
        {
            throw new PluginMisconfigurationException("Entry ID is null or empty. Please input a valid ID");
        }
        
        var client = new ContentfulClient(Creds, input.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () => await client.GetEntry(input.EntryId));

        var existingTags = entry.Metadata.Tags
            .Select(x => x.Sys.Id).ToArray();
       if (existingTags.Contains(input.TagId)) 
        {
            return;
        }
        
        var tags = entry.Metadata.Tags
            .Select(x => x.Sys.Id)
            .Append(input.TagId)
            .Select(x => new PropertiesRequest
            {
                Sys = new()
                {
                    Id = x,
                    Type = "Link",
                    LinkType = "Tag"
                }
            })
            .ToArray();

        await ReplaceTags(input.EntryId, input.Environment, entry.SystemProperties.Version, tags);
    }

    [Action("Remove tag from entry", Description = "Remove specific tag from an entry")]
    public async Task RemoveEntryTag([ActionParameter] EntryTagIdentifier input)
    {
        var client = new ContentfulClient(Creds, input.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () => 
                await client.GetEntry(input.EntryId));
        
        var existingTags = entry.Metadata.Tags
            .Select(x => x.Sys.Id).ToArray();
        if (!existingTags.Contains(input.TagId))
        {
            return;
        }

        var tags = entry.Metadata.Tags
            .Where(x => x.Sys.Id != input.TagId)
            .Select(x => new PropertiesRequest
            {
                Sys = new()
                {
                    Id = x.Sys.Id,
                    Type = "Link",
                    LinkType = "Tag"
                }
            })
            .ToArray();

        await ReplaceTags(input.EntryId, input.Environment, entry.SystemProperties.Version, tags);
    }

    #endregion

    #region Utils

    private Task ReplaceTags(string entryId, string? environment, int? version, PropertiesRequest[] tags)
    {
        var client = new ContentfulRestClient(Creds, environment);

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