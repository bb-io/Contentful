using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetJsonContentResponse
{
    [Display("JSON content")]
    public string? JsonContent { get; set; }

    [Display("Missing locales")]
    public List<string> MissingLocales { get; set; } = [];
}
