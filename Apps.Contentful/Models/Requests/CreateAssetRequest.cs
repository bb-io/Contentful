using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class CreateAssetRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public FileReference File { get; set; }
    public string? Filename { get; set; }
}