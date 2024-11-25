using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class GetFirstNotEmptyTextFieldContentRequest
{
    [Display("Field IDs")]
    public IEnumerable<string> FieldIds { get; set; } = default!;
}