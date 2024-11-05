namespace Apps.Contentful.Models.Dtos;

public class WorkflowDto
{
    public string StepId { get; set; } = string.Empty;
    
    public string PreviousStepId { get; set; } = string.Empty;
    
    public WorkflowSystemDto Sys { get; set; } = new ();
}

public class WorkflowSystemDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty;
    
    public int Version { get; set; }
    
    public WorkflowSystemIdentifierDto WorkflowDefinition { get; set; } = new ();
    
    public WorkflowSystemIdentifierDto Entity { get; set; } = new ();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}

public class WorkflowSystemIdentifierDto
{
    public SystemObjectDto Sys { get; set; } = default!;
}

public class SystemObjectDto
{
    public string Type { get; set; } = string.Empty;
    
    public string LinkType { get; set; } = string.Empty;
    
    public string Id { get; set; } = string.Empty;
    
    public int Version { get; set; }
}