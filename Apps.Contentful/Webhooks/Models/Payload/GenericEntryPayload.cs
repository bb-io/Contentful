using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Webhooks.Models.Payload;

public class GenericEntryPayload
{
    public SysObject Sys { get; set; }
    public JObject Fields { get; set; }
}

public class SysObject
{
    public string Id { get; set; }
    
    public EnvironmentPayload? Environment { get; set; }
}