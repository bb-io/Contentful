using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class UpdateEntryFromHtmlRequest
{
    [Display("Don't update reference fields")]
    public bool? DontUpdateReferenceFields { get; set; }
}
