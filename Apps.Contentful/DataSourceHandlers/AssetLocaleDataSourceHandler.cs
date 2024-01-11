using Apps.Contentful.DataSourceHandlers.Base;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class AssetLocaleDataSourceHandler : BaseAssetDataSourceHandler
{
    public AssetLocaleDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] AssetLocaleIdentifier identifier) :
        base(invocationContext, identifier.Environment)
    {
    }
}