using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class WorkflowIdentifier : EnvironmentIdentifier
{
    [Display("Workflow ID"), DataSource(typeof(WorkflowDataSource))]
    public string WorkflowId { get; set; } = string.Empty;
}