using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests
{
    public class FindWorkflowForEntryRequest : EntryIdentifier
    {
        [Display("Workflow definition ID"), DataSource(typeof(WorkflowDefinitionDataHandler))]
        public string WorkflowDefinitionId { get; set; } = string.Empty;
    }
}
