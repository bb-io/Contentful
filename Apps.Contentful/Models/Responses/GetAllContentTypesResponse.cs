using Apps.Contentful.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses
{
    public class GetAllContentTypesResponse
    {
        [Display("Content types")]
        public IEnumerable<ContentTypeDto> ContentTypes { get; set; }
    }
}
