using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Contentful.Models.Requests
{
    public class UpdateAssetFileRequest
    {
        public string AssetId { get; set; }

        public string SpaceId { get; set; }

        public string Locale { get; set; }

        public string? Filename { get; set; }

        public File File { get; set; }
    }
}
