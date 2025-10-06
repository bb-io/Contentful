using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos
{
    public class WorkflowCommentsListDto
    {
        [JsonProperty("items")]
        public List<WorkflowCommentItemDto> Items { get; set; } = new();
    }
    public class WorkflowCommentItemDto
    {
        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;

        [JsonProperty("sys")]
        public WorkflowCommentSysDto Sys { get; set; } = new();
    }

    public class WorkflowCommentSysDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("createdBy")]
        public WorkflowCommentLinkDto? CreatedBy { get; set; }
    }

    public class WorkflowCommentLinkDto
    {
        [JsonProperty("sys")]
        public WorkflowCommentLinkSysDto Sys { get; set; } = new();
    }

    public class WorkflowCommentLinkSysDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("linkType")]
        public string LinkType { get; set; } = string.Empty;
    }
}
