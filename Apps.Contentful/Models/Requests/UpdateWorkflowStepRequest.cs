using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class UpdateWorkflowStepRequest : WorkflowIdentifier
{
    [Display("Step ID")]
    public string StepId { get; set; } = string.Empty;
}