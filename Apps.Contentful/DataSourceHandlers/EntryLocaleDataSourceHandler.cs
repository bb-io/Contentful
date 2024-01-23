using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryLocaleDataSourceHandler : BaseEntryDataSourceHandler
{
    public EntryLocaleDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] EntryLocaleIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}