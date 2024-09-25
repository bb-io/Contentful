using Newtonsoft.Json;

namespace Apps.Contentful.Models.Dtos;

public class BaseSystem<T>
{
    [JsonProperty("sys")]
    public T Sys { get; set; } = default!;
}