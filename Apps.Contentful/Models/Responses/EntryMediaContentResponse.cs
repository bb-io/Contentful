using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Responses;

public class EntryMediaContentResponse
{
    [Display("Asset ID")]
    public string AssetId { get; set; }
    
    public FileReference File { get; set; }

    [Display("Missing locales")]
    public List<string> MissingLocales { get; set; }
}