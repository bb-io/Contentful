using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class AssetTagDataSourceHandler : BaseAssetDataSourceHandler
{
    public AssetTagDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] AssetTagIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}