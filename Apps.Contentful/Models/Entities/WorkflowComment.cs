using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Entities
{
    public class WorkflowComment
    {
        [Display("Comment ID")]
        public string CommentId { get; set; } = string.Empty;

        [Display("Body")]
        public string Body { get; set; } = string.Empty;

        [Display("Created at")]
        public DateTime? CreatedAt { get; set; }

        [Display("Updated at")]
        public DateTime? UpdatedAt { get; set; }

        [Display("Created by (ID)")]
        public string? CreatedById { get; set; }
    }
}
