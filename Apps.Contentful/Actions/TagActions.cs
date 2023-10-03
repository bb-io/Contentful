using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Requests.Tags;
using Apps.Contentful.Models.Responses.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.Actions;

[ActionList]
public class TagActions : ContentfulInvocable
{
    public TagActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

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
    public Task DeleteTag([ActionParameter] DeleteTagRequest input)
        => Client.DeleteContentTag(input.TagId, input.Version);
}