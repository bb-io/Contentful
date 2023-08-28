namespace Apps.Contentful.Models.Requests
{
    public class IsAssetLocalePresentRequest
    {
        public string AssetId { get; set; }

        public string SpaceId { get; set; }

        public string Locale { get; set; }
    }
}
