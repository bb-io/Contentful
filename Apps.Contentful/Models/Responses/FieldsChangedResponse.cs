using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class FieldsChangedResponse
{
    [Display("Entry ID")]
    public string EntryId { get; set; } = default!;

    public List<FieldObject> Fields { get; set; } = default!;
}

public class FieldObject
{
    [Display("Field ID")]
    public string FieldId { get; set; }

    public string Locale { get; set; }

    [Display("Field value")]
    public string FieldValue { get; set; }
}