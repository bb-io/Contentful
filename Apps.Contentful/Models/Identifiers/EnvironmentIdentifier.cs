using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EnvironmentIdentifier
{
    [DataSource(typeof(EnvironmentDataSourceHandler))]
    public string? Environment { get; set; }
}