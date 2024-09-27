using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryWithTaskDataSourceHandler : BaseEntryDataSourceHandler
{
    public EntryWithTaskDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] EntryTaskIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}