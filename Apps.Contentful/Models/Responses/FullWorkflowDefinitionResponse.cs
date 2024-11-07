using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class FullWorkflowDefinitionResponse
{
    [Display("Workflow definition ID")]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    [Display("Workflow definition name")]
    public string Name { get; set; } = string.Empty;

    [Display("Workflow definition description")]
    public string Description { get; set; } = string.Empty;
    
    [Display("Steps")]
    public List<WorkflowStep> Steps { get; set; } = new();
}