using Apps.Contentful.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Entities;

public class EntryTaskEntity(TaskDto dto)
{
    [Display("Entry task ID")]
    public string Id { get; set; } = dto.Sys.Id;

    public string Body { get; set; } = dto.Body;

    public string Status { get; set; } = dto.Status;

    [Display("Due date")]
    public DateTime DueDate { get; set; } = dto.DueDate ?? DateTime.MinValue;

    [Display("Assigned to")]
    public string AssignedTo { get; set; } = dto.AssignedTo.Sys.Id;

    [Display("Parent entry ID")]
    public string ParentEntryId { get; set; } = dto.Sys.ParentEntity.Sys.Id;

    [Display("Created by")]
    public ObjectEntity CreatedBy { get; set; } = new()
    {
        Id = dto.Sys.CreatedBy.Sys.Id,
        LinkType = dto.Sys.CreatedBy.Sys.LinkType
    };

    [Display("Updated by")]
    public ObjectEntity UpdatedBy { get; set; } = new()
    {
        Id = dto.Sys.UpdatedBy.Sys.Id,
        LinkType = dto.Sys.UpdatedBy.Sys.LinkType
    };

    [Display("Created at")]
    public DateTime CreatedAt { get; set; } = dto.Sys.CreatedAt ?? DateTime.MinValue;

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; } = dto.Sys.UpdatedAt ?? DateTime.MinValue;

    public string Version { get; set; } = dto.Sys.Version.ToString();
}

public class ObjectEntity
{
    public string Id { get; set; }
    
    [Display("Link type")]
    public string LinkType { get; set; }
}