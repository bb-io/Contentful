using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryWithAssetDataSourceHandler : BaseEntryDataSourceHandler
{
    public EntryWithAssetDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] AssetEntryLocaleIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}