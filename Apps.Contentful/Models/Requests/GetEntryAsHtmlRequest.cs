using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class GetEntryAsHtmlRequest
{
    [Display("Get content of linked entries")]
    public bool? GetLinkedContent { get; set; }
}