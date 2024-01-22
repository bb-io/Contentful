using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Webhooks.Models.Payload;

public class AssetPayload
{
    public SysObject Sys { get; set; }

    public AssetFields Fields { get; set; }
}

public class AssetFields
{
    public JObject File { get; set; }
}

public class AssetFileInfo
{
    public string Locale { get; set; }
    
    [Display("URL")]
    public string Url { get; set; }
    
    [Display("File name")]
    public string FileName { get; set; }
    
    [Display("Content type")]
    public string ContentType { get; set; }
}