using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class WorkflowStepFilterRequest : WorkflowDefinitionOptionalIdentifier
{
    [Display("Current step ID"), DataSource(typeof(WorkflowStepDataHandler))]
    public string? CurrentStepId { get; set; }
    
    [Display("Current step name", Description = "Filter by the name of the current step")]
    public string? CurrentStepName { get; set; }
}