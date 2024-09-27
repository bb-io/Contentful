using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class AssetIdentifier : EnvironmentIdentifier
{
    [Display("Asset ID")]
    [DataSource(typeof(AssetDataSourceHandler))]
    public string AssetId { get; set; }
}