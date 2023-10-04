using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Webhooks.Payload;

public class GenericEntryPayload
{
    public SysObject Sys { get; set; }
    public JObject Fields { get; set; }
}

public class SysObject
{
    public string Id { get; set; }
}