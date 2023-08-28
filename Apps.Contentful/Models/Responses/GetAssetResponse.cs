using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Contentful.Models.Responses
{
    public class GetAssetResponse
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public File? File { get; set; }
    }
}
