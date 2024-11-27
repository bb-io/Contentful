namespace Apps.Contentful.Models.Responses;

public class WorkflowsResponse
{
    public List<WorkflowDefinitionResponse> Workflows { get; set; }

    public double TotalCount { get; set; }

    public WorkflowsResponse(IEnumerable<WorkflowDefinitionResponse> workflow)
    {
        Workflows = workflow.ToList();
        TotalCount = Workflows.Count;
    }
}