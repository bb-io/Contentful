using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Models.Entities;

public class TagEntity
{
    [Display("Tag ID")] public string Id { get; set; }

    public string Name { get; set; }

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    public int? Version { get; set; }

    public TagEntity(ContentTag tag)
    {
        Id = tag.SystemProperties.Id;
        Name = tag.Name;
        CreatedAt = tag.SystemProperties.CreatedAt;
        Version = tag.SystemProperties.Version;
    }
}