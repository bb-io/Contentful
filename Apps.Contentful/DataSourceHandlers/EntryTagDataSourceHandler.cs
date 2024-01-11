using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryTagDataSourceHandler : BaseEntryDataSourceHandler
{
    public EntryTagDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] EntryTagIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}