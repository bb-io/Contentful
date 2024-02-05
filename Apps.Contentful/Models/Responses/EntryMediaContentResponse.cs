using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Responses;

public class EntryMediaContentResponse
{
    public string AssetId { get; set; }
    
    public FileReference File { get; set; }
}