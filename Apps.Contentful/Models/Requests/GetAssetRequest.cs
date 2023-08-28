namespace Apps.Contentful.Models.Requests
{
    public class GetAssetRequest
    {
        public string AssetId { get; set; }

        public string Locale { get; set; }

        public string SpaceId { get; set; }
    }
}
