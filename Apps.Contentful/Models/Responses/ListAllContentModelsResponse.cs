using Apps.Contentful.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class ListAllContentModelsResponse
{
    [Display("Content models")]
    public IEnumerable<ContentModelDto> ContentModels { get; set; }
}