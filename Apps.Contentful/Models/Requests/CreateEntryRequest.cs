using Apps.Contentful.Dtos;

namespace Apps.Contentful.Models.Requests
{
    public class CreateEntryRequest
    {
        public string ContentTypeId { get; set; }

        public string SpaceId { get; set; }

        public TestDto Content { get; set; }
    }
}
