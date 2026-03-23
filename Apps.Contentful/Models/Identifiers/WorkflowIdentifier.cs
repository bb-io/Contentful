using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Models.Identifiers;

public class WorkflowIdentifier : EnvironmentIdentifier
{
    [Display("Workflow ID"), DataSource(typeof(WorkflowDataSource))]
    public string WorkflowId { get; set; } = string.Empty;

    public WorkflowIdentifier Validate()
    {
        if (string.IsNullOrWhiteSpace(WorkflowId))
            throw new PluginMisconfigurationException("Please fill in the 'Workflow ID' input");

        return this;
    }
}