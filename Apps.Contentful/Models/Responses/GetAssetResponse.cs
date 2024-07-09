using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Responses;

public class GetAssetResponse
{
    public string? Title { get; set; }

    public string? Description { get; set; }
    
    public IEnumerable<string> Tags { get; set; }

    public FileReference? File { get; set; }

    public string? Locale { get; set; }
}