using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Webhooks.Models.Inputs;

public class WebhookInput
{
    [DataSource(typeof(EnvironmentDataSourceHandler))]
    public string? Environment { get; set; }
}