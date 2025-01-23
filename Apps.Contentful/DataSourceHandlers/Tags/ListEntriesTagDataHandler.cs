using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers.Tags;

public class ListEntriesTagDataHandler(InvocationContext invocationContext, [ActionParameter] ListEntriesRequest input)
    : TagDataHandler(invocationContext, new()
    {
        Environment = input.Environment
    });