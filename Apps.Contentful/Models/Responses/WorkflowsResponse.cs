namespace Apps.Contentful.Models.Responses;

public class WorkflowsResponse
{
    public List<WorkflowResponse> Workflows { get; set; }

    public double TotalCount { get; set; }

    public WorkflowsResponse(IEnumerable<WorkflowResponse> workflow)
    {
        Workflows = workflow.ToList();
        TotalCount = Workflows.Count;
    }
}