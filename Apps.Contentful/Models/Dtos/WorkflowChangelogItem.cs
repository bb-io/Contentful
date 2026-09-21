using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos
{
    public class WorkflowChangelogItem
    {
        [JsonProperty("eventAt")]
        public DateTime EventAt { get; set; }

        [JsonProperty("eventBy")]
        public EventBy? EventBy { get; set; }

        [JsonProperty("stepId")]
        public string StepId { get; set; } = string.Empty;

        [JsonProperty("stepName")]
        public string StepName { get; set; } = string.Empty;

        public string? UserId => EventBy?.Sys?.Id;
    }

    public class EventBy
    {
        [JsonProperty("sys")]
        public SysLink? Sys { get; set; }
    }

    public class SysLink
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("linkType")]
        public string LinkType { get; set; } = string.Empty;
    }
}
