namespace Apps.Contentful.Models.Requests
{
    public class GetEntryRequest
    {
        public string EntryId { get; set; }

        public string FieldId { get; set; }

        public string SpaceId { get; set; }

        public string Locale { get; set; }
    }
}
