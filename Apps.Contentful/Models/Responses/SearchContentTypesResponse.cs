using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class SearchContentTypesResponse
{
    [Display("Content types")]
    public List<ContentTypeResponse> ContentTypes { get; set; } = new();
    
    [Display("Total count")]
    public int TotalCount { get; set; }
}