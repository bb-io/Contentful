using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class WorkflowsResponse
{
    public List<WorkflowDefinitionResponse> Workflows { get; set; }

    [Display("Total count")]
    public double TotalCount { get; set; }

    public WorkflowsResponse(IEnumerable<WorkflowDefinitionResponse> workflow)
    {
        Workflows = workflow.ToList();
        TotalCount = Workflows.Count;
    }
}