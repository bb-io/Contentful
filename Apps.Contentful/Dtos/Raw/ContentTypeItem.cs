using Newtonsoft.Json;

namespace Apps.Contentful.Dtos.Raw
{
    public class ContentTypeItem
    {
        [JsonProperty("sys", NullValueHandling = NullValueHandling.Ignore)]
        public SystemProperties SystemProperties { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DisplayField { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public List<ContentTypeField>? Fields { get; set; }
    }

    public class SystemProperties
    {
        public string Id { get; set; }
    }

    public class ContentTypeField
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool Required { get; set; }

        public bool Localized { get; set; }

        public string LinkType { get; set; }

        public bool Disabled { get; set; }

        public bool Omitted { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public ContentTypeFieldSchema? Items { get; set; }
    }

    public class ContentTypeFieldSchema
    {
        public string LinkType { get; set; }
    }
}
