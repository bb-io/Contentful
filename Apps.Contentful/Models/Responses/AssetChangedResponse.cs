using Apps.Contentful.Webhooks.Payload;

namespace Apps.Contentful.Models.Responses
{
    public class AssetChangedResponse
    {
        public string AssetId { get; set; }

        public List<AssetFileInfo> FilesInfo { get; set; }
    }
}
