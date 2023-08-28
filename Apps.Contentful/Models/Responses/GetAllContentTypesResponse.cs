using Apps.Contentful.Dtos;

namespace Apps.Contentful.Models.Responses
{
    public class GetAllContentTypesResponse
    {
        public IEnumerable<ContentTypeDto> ContentTypes { get; set; }
    }
}
