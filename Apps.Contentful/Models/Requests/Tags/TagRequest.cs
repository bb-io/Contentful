using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests.Tags;

public class TagRequest : EnvironmentIdentifier
{
    [Display("Tag")]
    [DataSource(typeof(TagDataHandler))]
    public string TagId { get; set; }
}