using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses
{
    public class FieldsChangedResponse
    {
        [Display("Entry")]
        public string EntryId { get; set; }

        public List<FieldObject> Fields { get; set; }
    }

    public class FieldObject
    {
        [Display("Field ID")]
        public string FieldId { get; set; }

        public string Locale { get; set; }

        [Display("Field value")]
        public string FieldValue { get; set; }
    }
}
