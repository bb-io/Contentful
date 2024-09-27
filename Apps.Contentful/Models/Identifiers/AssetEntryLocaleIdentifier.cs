using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class AssetEntryLocaleIdentifier : LocaleIdentifier
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryWithAssetDataSourceHandler))]
    public string EntryId { get; set; }
    
    [Display("Asset ID")]
    [DataSource(typeof(AssetWithEntryDataSourceHandler))]
    public string AssetId { get; set; }
}