using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class WorkflowResponse
{    
    [Display("Workflow ID")]
    public string WorkflowId { get; set; } = string.Empty;
    
    [Display("Workflow definition ID")]
    public string WorkflowDefinitionId { get; set; } = string.Empty;
    
    [Display("Current step ID")] 
    public string StepId { get; set; } = string.Empty;

    [Display("Entity ID")]
    public string EntityId { get; set; } = string.Empty;

    public int Version { get; set; }
}