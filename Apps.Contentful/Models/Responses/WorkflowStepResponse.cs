using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class WorkflowStepResponse : WorkflowResponse
{
    [Display("Current step name")]
    public string CurrentStepName { get; set; } = string.Empty;
}