using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetIdsFromHtmlResponse
{
    [Display("Entry ID")]
    public string EntryId { get; set; } = string.Empty;
    
    [Display("Field ID")]
    public string FieldId { get; set; } = string.Empty;
}