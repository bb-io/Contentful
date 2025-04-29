using Contentful.Core.Models;
using Newtonsoft.Json;

namespace Apps.Contentful.Models.Entities;

public class CustomQuoteEntity : IContent
{
    [JsonProperty("data")]
    public object Data { get; set; } = default!;

    [JsonProperty("content")]
    public IEnumerable<object> Content { get; set; } = new List<object>();

    [JsonProperty("nodeType")]
    public string NodeType { get; set; } = string.Empty;
}
