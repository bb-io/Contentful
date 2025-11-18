using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models
{
    public class ContentProcessingError
    {
        [Display("Entry ID")]
        public string EntryId { get; set; } = default!;

        [Display("Parent ID")]
        public string? ParentEntryId { get; set; }

        [Display("Error message")]
        public string ErrorMessage { get; set; } = default!;
    }
}
