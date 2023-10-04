using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Apps.Contentful.Constants;

public static class JsonConfig
{
    public static JsonSerializerSettings Settings => new()
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };
}