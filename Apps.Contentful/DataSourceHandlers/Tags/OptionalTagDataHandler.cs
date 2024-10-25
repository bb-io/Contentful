using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers.Tags;

public class OptionalTagDataHandler : TagDataHandler
{
    public OptionalTagDataHandler(InvocationContext invocationContext, [ActionParameter] OptionalTagListIdentifier tag)
        : base(invocationContext, new()
        {
            Environment = tag.Environment
        })
    {
    }
}