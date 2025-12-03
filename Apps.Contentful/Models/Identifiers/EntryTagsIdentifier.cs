using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EntryTagsIdentifier : MultipleTagRequest
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryTagDataSourceHandler))]
    public string EntryId { get; set; }
}