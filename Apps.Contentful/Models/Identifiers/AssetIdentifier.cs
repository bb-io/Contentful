using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class AssetIdentifier
{
    [Display("Asset")]
    [DataSource(typeof(AssetDataSourceHandler))]
    public string Id { get; set; }
}