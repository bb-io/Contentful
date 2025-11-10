using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetNumberContentResponse
{
    [Display("Number content")]
    public double NumberContent { get; set; }
}