using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests.Tags;

public class CreateTagRequest
{
    public string Name { get; set; }
    
    [Display("Tag ID")]
    public string TagId { get; set; }
    
    [Display("Is public")]
    public bool IsPublic { get; set; }
}