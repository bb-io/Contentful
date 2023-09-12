using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models;

namespace Apps.Contentful.Dtos;

public class FieldDto
{
    public FieldDto(Field field)
    {
        FieldId = field.Id;
        Type = field.Type;
        IsLocalizable = field.Localized;
        IsRequired = field.Required;
    }
    
    [Display("Field")]
    public string FieldId { get; set; }
    
    public string Type { get; set; }
    
    [Display("Is localizable")]
    public bool IsLocalizable { get; set; }
    
    [Display("Is required")]
    public bool IsRequired { get; set; }
}