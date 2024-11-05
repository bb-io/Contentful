using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class WorkflowStepFilterRequest
{
    [Display("Workflow definition ID"), DataSource(typeof(WorkflowDefinitionDataHandler))]
    public string? WorkflowDefinitionId { get; set; }
    
    [Display("Current step ID"), DataSource(typeof(WorkflowStepDataHandler))]
    public string? CurrentStepId { get; set; }
    
    [Display("Previous step ID"), DataSource(typeof(WorkflowStepDataHandler))]
    public string? PreviousStepId { get; set; }
}