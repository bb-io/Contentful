using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos;

public class BaseDto
{
    [JsonProperty("linkType")]
    public string LinkType { get; set; } = default!;
    
    [JsonProperty("type")]
    public string Type { get; set; } = default!;
    
    [JsonProperty("id")]
    public string Id { get; set; } = default!;
}