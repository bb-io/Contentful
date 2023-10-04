using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Contentful.Models.Requests;

public class UpdateAssetFileRequest
{
    public string? Filename { get; set; }
    public File File { get; set; }
}