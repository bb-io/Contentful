using Contentful.Core.Models;
using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos;

public class RestEntryDto<T>
{
    [JsonProperty("sys")]
    public SystemProperties SystemProperties { get; set; }

    [JsonProperty("metadata")]
    public ContentfulMetadata Metadata { get; set; }
    
    public T Fields { get; set; }
}