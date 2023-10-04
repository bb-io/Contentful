using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetNumberContentResponse
{
    [Display("Number content")]
    public int NumberContent { get; set; }
}