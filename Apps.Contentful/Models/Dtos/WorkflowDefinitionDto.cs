using Apps.Contentful.Models.Responses;

namespace Apps.Contentful.Models.Dtos;

public class WorkflowDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public List<WorkflowStep> Steps { get; set; } = new();

    public SystemObjectDto Sys { get; set; } = new();
}