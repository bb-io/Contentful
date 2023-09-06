using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Contentful.Models.Requests
{
    public class CreateAssetRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public File File { get; set; }
        public string? Filename { get; set; }
    }
}
