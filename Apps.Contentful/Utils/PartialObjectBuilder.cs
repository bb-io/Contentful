using Contentful.Core.Models;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Utils;

public static class PartialObjectBuilder
{
    public static object Build(Entry<dynamic> source, string locale)
    {
        if (source?.Fields == null)
            return new { };

        var result = new Dictionary<string, object>();
        var fields = source.Fields as JObject;
        
        if (fields == null)
            return new { };

        foreach (var field in fields)
        {
            var fieldValue = field.Value as JObject;
            if (fieldValue != null && fieldValue.ContainsKey(locale))
            {
                result[field.Key] = fieldValue[locale]!;
            }
        }

        return result;
    }
}