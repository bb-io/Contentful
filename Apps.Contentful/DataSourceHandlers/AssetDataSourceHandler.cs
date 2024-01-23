using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class AssetDataSourceHandler : BaseAssetDataSourceHandler
{
    public AssetDataSourceHandler(InvocationContext invocationContext, [ActionParameter] AssetIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}