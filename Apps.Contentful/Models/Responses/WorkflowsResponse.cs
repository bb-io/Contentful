namespace Apps.Contentful.Models.Responses;

public class WorkflowsResponse
{
    public List<WorkflowStepResponse> Workflows { get; set; }

    public double TotalCount { get; set; }

    public WorkflowsResponse(IEnumerable<WorkflowStepResponse> workflow)
    {
        Workflows = workflow.ToList();
        TotalCount = Workflows.Count;
    }
}