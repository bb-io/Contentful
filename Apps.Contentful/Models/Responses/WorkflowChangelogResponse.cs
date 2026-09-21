
using Apps.Contentful.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses
{

public class WorkflowChangelogResponse
{
    [Display("Changelog items")]
    public List<ChangelogEntryDto> Items { get; set; }

    public WorkflowChangelogResponse(IEnumerable<WorkflowChangelogItem> rawCollection)
    {
        Items = rawCollection.Select(item => new ChangelogEntryDto
        {
            Date = item.EventAt,
            UserId = item.UserId,
            StepId = item.StepId,
            StepName = item.StepName
        }).ToList();
    }
}

public class ChangelogEntryDto
{
    [Display("Date")]
    public DateTime Date { get; set; }

    [Display("User ID")]
    public string? UserId { get; set; }

    [Display("Step ID")]
    public string StepId { get; set; } = string.Empty;

    [Display("Step name")]
    public string StepName { get; set; } = string.Empty;
}
}
