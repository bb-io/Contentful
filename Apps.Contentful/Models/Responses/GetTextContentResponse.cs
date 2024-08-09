using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetTextContentResponse
{
    [Display("Text content")]
    public string? TextContent { get; set; }
}