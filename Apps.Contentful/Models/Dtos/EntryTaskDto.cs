using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos;

public class EntryTaskDto : BaseSystem<SysDto>
{
    [JsonProperty("userAgent")]
    public string UserAgent { get; set; } = default!;
}

public class SysDto
{
    [JsonProperty("user")]
    public BaseSystem<BaseDto> User { get; set; } = default!;
    
    public BaseSystem<BaseDto> Environment { get; set; } = default!;
    
    public BaseSystem<BaseDto> Organization { get; set; } = default!;
    
    public BaseSystem<BaseDto> Space { get; set; } = default!;
    
    public TaskDto NewTask { get; set; } = default!;
}

public class TaskDto
{
    [JsonProperty("body")]
    public string Body { get; set; } = default!;
    
    [JsonProperty("assignedTo")]
    public BaseSystem<BaseDto> AssignedTo { get; set; } = default!;
    
    public string Status { get; set; } = default!;
    
    public DateTime? DueDate { get; set; } 
    
    public SystemTaskDto Sys { get; set; } = default!;
}

public class SystemTaskDto
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("parentEntity")]
    public BaseSystem<BaseDto> ParentEntity { get; set; } = default!;
    
    [JsonProperty("space")]
    public BaseSystem<BaseDto> Space { get; set; } = default!;
    
    [JsonProperty("environment")]
    public BaseSystem<BaseDto> Environment { get; set; } = default!;
    
    [JsonProperty("createdBy")]
    public BaseSystem<BaseDto> CreatedBy { get; set; } = default!;
    
    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonProperty("updatedBy")]
    public BaseSystem<BaseDto> UpdatedBy { get; set; } = default!;
    
    [JsonProperty("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonProperty("version")]
    public int Version { get; set; }
}
