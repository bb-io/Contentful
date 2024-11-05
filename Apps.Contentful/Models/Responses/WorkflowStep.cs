using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Contentful.Models.Responses;

public class WorkflowStep
{
    [Display("Step ID"), JsonProperty("id")]
    public string StepId { get; set; } = string.Empty;

    [Display("Name")]
    public string Name { get; set; } = string.Empty;

    [Display("Description")]
    public string Description { get; set; } = string.Empty;

    public List<string> Annotations { get; set; } = new();
}