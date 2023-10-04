using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models;

namespace Apps.Contentful.Dtos;

public class ContentModelDto
{
    public ContentModelDto(ContentType contentType)
    {
        ContentModelId = contentType.SystemProperties.Id;
        ContentModelName = contentType.Name;
        Fields = contentType.Fields.Select(f => new FieldDto(f));
    }
         
    [Display("Content model")]
    public string ContentModelId { get; set; }
        
    [Display("Content model name")]
    public string ContentModelName { get; set; }
        
    public IEnumerable<FieldDto> Fields { get; set; }
}