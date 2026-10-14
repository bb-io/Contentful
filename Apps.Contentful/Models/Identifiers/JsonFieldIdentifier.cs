using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class JsonFieldIdentifier
{
    [Display("Field ID"), DataSource(typeof(JsonFieldDataHandler))]
    public string FieldId { get; set; } = string.Empty;
}
