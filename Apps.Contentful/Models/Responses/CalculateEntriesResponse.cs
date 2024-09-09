using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class CalculateEntriesResponse
{
    [Display("Entries total count")]
    public double TotalCount { get; set; }
}