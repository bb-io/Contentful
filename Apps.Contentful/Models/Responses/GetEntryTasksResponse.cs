using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetEntryTasksResponse
{
    [Display("Entry tasks")]
    public List<EntryTaskEntity> EntryTasks { get; set; } = new();

    [Display("Total count")]
    public double TotalCount { get; set; }
}