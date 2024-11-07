using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class WorkflowDefinitionOptionalIdentifier
{
    [Display("Workflow definition ID"), DataSource(typeof(WorkflowDefinitionDataHandler))]
    public string? WorkflowDefinitionId { get; set; }
}