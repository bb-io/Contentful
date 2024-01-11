using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class AssetTagIdentifier : TagRequest
{
    [Display("Asset")]
    [DataSource(typeof(AssetTagDataSourceHandler))]
    public string AssetId { get; set; }
}