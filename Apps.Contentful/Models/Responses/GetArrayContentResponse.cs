using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetArrayContentResponse
{
    [Display("Array content")]
    public List<string>? ArrayContent { get; set; }

    [Display("Missing locales")]
    public List<string> MissingLocales { get; set; }
}