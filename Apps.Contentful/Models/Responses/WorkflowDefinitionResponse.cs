using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class WorkflowDefinitionResponse
{
    [Display("Workflow ID")] 
    public string WorkflowId { get; set; } = string.Empty;
    
    [Display("Workflow definition ID")]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    [Display("Workflow definition name")]
    public string Name { get; set; } = string.Empty;

    [Display("Workflow definition description")]
    public string Description { get; set; } = string.Empty;

    [Display("Entry ID")]
    public string EntryId { get; set; } = string.Empty;

    [Display("Current step")]
    public WorkflowStep CurrentStep { get; set; } = new ();
    
    [Display("Previous step")]
    public WorkflowStep? PreviousStep { get; set; } = new ();
    
    [Display("Next step")]
    public WorkflowStep? NextStep { get; set; } = new ();
}