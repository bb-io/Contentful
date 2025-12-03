using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.DataSourceHandlers.Tags;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests.Tags;

public class MultipleTagRequest : EnvironmentIdentifier
{
    [Display("Tag IDs")]
    [DataSource(typeof(TagDataHandler))]
    public IEnumerable<string> TagIds { get; set; }
}