using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers.Tags;

public class ListEntriesTagDataHandler : TagDataHandler
{
    public ListEntriesTagDataHandler(InvocationContext invocationContext, [ActionParameter] ListEntriesRequest input) : base(invocationContext, new()
    {
        Environment = input.Environment
    })
    {
    }
}