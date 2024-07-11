using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class ContentTypeResponse
{
    [Display("Content type ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Content type name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display("Display field")] 
    public string DisplayField { get; set; } = string.Empty;
    
    [Display("Fields")]
    public List<FieldResponse> Fields { get; set; } = new();

    public string Locale { get; set; } = string.Empty;
}

public class FieldResponse
{
    [Display("Field ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Field name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Field type")]
    public string Type { get; set; } = string.Empty;
    
    [Display("Localized")]
    public bool Localized { get; set; }
    
    [Display("Required")]
    public bool Required { get; set; }
    
    [Display("Disabled")]
    public bool Disabled { get; set; }
    
    [Display("Omitted")]
    public bool Omitted { get; set; }
}