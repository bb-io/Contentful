using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests.Tags;

public class DeleteTagRequest
{
    [Display("Tag")]
    [DataSource(typeof(TagDataHandler))]
    public string TagId { get; set; }
    
    public int Version { get; set; }
}