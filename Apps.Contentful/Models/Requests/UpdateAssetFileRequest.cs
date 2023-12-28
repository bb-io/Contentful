using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class UpdateAssetFileRequest
{
    public string? Filename { get; set; }
    public FileReference File { get; set; }
}