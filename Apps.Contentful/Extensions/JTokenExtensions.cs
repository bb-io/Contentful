using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Extensions;

public static class JTokenExtensions
{
    public static JToken Escape(this JToken token)
    {
        return JToken.Parse(JsonConvert.SerializeObject(token));
    }
}