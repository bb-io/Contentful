using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class WorkflowStepFilterRequest : WorkflowDefinitionOptionalIdentifier
{
    [Display("Current step name", Description = "Filter by the name of the current step")]
    public IEnumerable<string>? CurrentStepName { get; set; }
}
