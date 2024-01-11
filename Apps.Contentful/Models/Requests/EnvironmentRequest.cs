using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class EnvironmentRequest
{
    [DataSource(typeof(EnvironmentDataSourceHandler))]
    public string? Environment { get; set; }
}