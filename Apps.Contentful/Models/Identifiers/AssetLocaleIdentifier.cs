using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class AssetLocaleIdentifier : LocaleIdentifier
{
    [Display("Asset")]
    [DataSource(typeof(AssetLocaleDataSourceHandler))]
    public string AssetId { get; set; }
}