using System.Web;
using Apps.Contentful.HtmlHelpers.Constants;
using Apps.Contentful.Models;
using Apps.Contentful.Models.Exceptions;
using Contentful.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Apps.Contentful.HtmlHelpers;

public static class EntryToJsonConverter
{
    public static List<EntryHtmlContentDto> GetEntriesInfo(string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        return htmlDocument.DocumentNode.Descendants()
            .Where(x => x.Attributes[ConvertConstants.EntryIdAttribute] is not null)
            .Select(x => new EntryHtmlContentDto(x.Attributes[ConvertConstants.EntryIdAttribute].Value, x))
            .ToList();
    }

    public static void ToJson(Entry<object> entry, HtmlNode html, string locale, ContentType contentType, bool doNotUpdateReferenceFields)
    {
        var entryFields = (JObject)entry.Fields;

        var elements = html.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element).ToList();
        elements.ForEach(x => UpdateEntryFieldFromHtml(x, entryFields, locale, contentType, doNotUpdateReferenceFields));
    }

    private static void UpdateEntryFieldFromHtml(HtmlNode htmlNode, JObject entryFields, string locale, ContentType contentType, bool doNotUpdateReferenceFields)
    {
        var fieldId = htmlNode.Attributes[ConvertConstants.FieldIdAttribute].Value;
        var fieldType = htmlNode.Attributes[ConvertConstants.FieldTypeAttribute].Value;

        if(doNotUpdateReferenceFields == true)
        {
            var field = contentType.Fields.FirstOrDefault(x => x.Id == fieldId);
            if (field != null && (field.Type == "Link" || (field.Type == "Array" && field.Items.LinkType == "Entry")))
            {
                return;
            }
        }

        void SetEntryFieldValue(string id, object newValue)
        {
            var jTokenValue = JToken.FromObject(newValue);
            if (entryFields.TryGetValue(id, out var entryField))
            {
                if (entryFields[id] is JObject field && field.TryGetValue(locale, out _))
                {
                    entryFields[id]![locale] = jTokenValue;
                }
                else
                {
                    var jObject = entryField as JObject;
                    if (jObject != null)
                    {
                        jObject.Add(locale, jTokenValue);
                        entryFields[id] = jObject;
                    }
                    else
                    {
                        var localeJObject = new JObject { { locale, jTokenValue } };
                        entryFields[id] = localeJObject;
                    }
                }
            }
            else
            {
                var localeJObject = new JObject { { locale, jTokenValue } };
                entryFields.Add(id, localeJObject);
            }
        }

        switch (fieldType)
        {
            case "Integer":
                if (!int.TryParse(htmlNode.InnerText, out var intValue))
                {
                    throw new FieldConversionException(fieldId, htmlNode.InnerText, "integer");
                }
                SetEntryFieldValue(fieldId, intValue);
                break;
            case "Number":
                if (!decimal.TryParse(htmlNode.InnerText, out var decimalValue))
                {
                    throw new FieldConversionException(fieldId, htmlNode.InnerText, "number");
                }
                var finalValue = decimalValue == Decimal.Floor(decimalValue)
                    ? Decimal.ToInt64(decimalValue)
                    : decimalValue;
                SetEntryFieldValue(fieldId, finalValue);
                break;
            case "Symbol":
                SetEntryFieldValue(fieldId, HttpUtility.HtmlDecode(htmlNode.InnerText));
                break;
            case "Text":
                SetEntryFieldValue(fieldId, HttpUtility.HtmlDecode(htmlNode.InnerText));
                break;
            case "Object":
                var parsedObject = ParseJsonObjectFromHtmlNode(htmlNode);
                SetEntryFieldValue(fieldId, parsedObject);
                break;
            case "Location":
                var jsonValue = htmlNode.Attributes["data-contentful-json-value"].Value;
                var jsonObject = JToken.Parse(HttpUtility.HtmlDecode(jsonValue));
                SetEntryFieldValue(fieldId, jsonObject);
                break;
            case "Boolean":
                var boolValue = Convert.ToBoolean(htmlNode.Attributes["data-contentful-bool-value"].Value);
                SetEntryFieldValue(fieldId, boolValue);
                break;
            case "RichText":
                var richTextValue = ParseToRichText(htmlNode);
                SetEntryFieldValue(fieldId, richTextValue);
                break;
            case "Link":
                var linkType = htmlNode.Attributes["data-contentful-link-type"].Value;
                var id = htmlNode.Attributes["data-contentful-link-id"].Value;
                var localized = htmlNode.Attributes["data-contentful-localized"]?.Value;
                var linkData = new
                {
                    sys = new
                    {
                        type = "Link",
                        linkType,
                        id
                    }
                };
                if (linkType == "Asset")
                {
                    SetEntryFieldValue(fieldId, JObject.Parse(JsonConvert.SerializeObject(linkData)));
                }
                else
                {
                    if (localized != null && localized.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {                        
                        SetEntryFieldValue(fieldId, JObject.Parse(JsonConvert.SerializeObject(linkData)));
                    }
                    else
                    {
                        var fieldObject = entryFields[fieldId] as JObject;
                        if (fieldObject != null)
                        {
                            fieldObject.Remove(locale);
                        }
                    }
                }

                break;
            case "Array":
                if (htmlNode.Attributes.Any(a => a.Name == "data-contentful-link-type"))
                {
                    linkType = htmlNode.Attributes["data-contentful-link-type"].Value;
                    var itemIds = htmlNode.Attributes["data-contentful-link-ids"].Value.Split(",");
                    var arrayLocalized = htmlNode.Attributes["data-contentful-localized"]?.Value;
                    var arrayData = itemIds.Select(id => new
                    {
                        sys = new
                        {
                            type = "Link",
                            linkType,
                            id
                        }
                    });

                    if (linkType == "Entry")
                    {
                        if (arrayLocalized != null &&
                            arrayLocalized.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            SetEntryFieldValue(fieldId, JArray.Parse(JsonConvert.SerializeObject(arrayData)));
                        }
                        else
                        {
                            var fieldObject = entryFields[fieldId] as JObject;
                            if (fieldObject != null)
                            {
                                if (fieldObject.ContainsKey(locale))
                                {
                                    fieldObject.Remove(locale);
                                }
                            }
                        }
                    }
                    else
                    {
                        var jArray = JArray.Parse(JsonConvert.SerializeObject(arrayData));
                        SetEntryFieldValue(fieldId, jArray);
                    }
                }
                else
                {
                    var arrayContent = htmlNode
                        .Descendants()
                        .Where(x => x.Name == HtmlConstants.Li)
                        .Select(x => HttpUtility.HtmlDecode(x.InnerText));

                    SetEntryFieldValue(fieldId, JArray.FromObject(arrayContent));
                }

                break;
        }
    }

    private static JToken ParseJsonObjectFromHtmlNode(HtmlNode htmlNode)
    {
        var richTextAttribute = htmlNode.Attributes["data-rich-text"];
        if(richTextAttribute != null && richTextAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return ParseToRichText(htmlNode);
        }
        
        var arrayJsonAttribute = htmlNode.Attributes["data-array-json-object"];
        if (arrayJsonAttribute != null && arrayJsonAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            var jsonArray = new JArray();
            
            // Find all customField divs
            var customFieldNodes = htmlNode.SelectNodes(".//div[@data-field='customField']");
            if (customFieldNodes != null)
            {
                foreach (var customFieldNode in customFieldNodes)
                {
                    var jsonValue = customFieldNode.Attributes["data-contentful-json-value"]?.Value;
                    if (jsonValue != null)
                    {
                        // Parse the base JSON object from the attribute
                        var baseJsonObject = JObject.Parse(HttpUtility.HtmlDecode(jsonValue));
                        
                        // Process child elements to update values
                        var childElements = customFieldNode.SelectNodes(".//div[@data-path]");
                        if (childElements != null)
                        {
                            foreach (var childElement in childElements)
                            {
                                var path = childElement.Attributes["data-path"]?.Value;
                                var field = childElement.Attributes["data-field"]?.Value;

                                var spitedPath = path.Split('.');
                                var skipedFirstTwo = spitedPath.Skip(2).ToList();
                                var pathSegments = string.Join(".", skipedFirstTwo);
                                
                                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(field))
                                {   
                                    var token = baseJsonObject.SelectToken(pathSegments);            
                                    if (token != null)
                                    {
                                        var newValue = childElement.InnerText.Trim();
                                        if (!string.IsNullOrEmpty(newValue))
                                        {
                                            token.Replace(newValue);
                                        }
                                    }              
                                }
                            }
                        }
                        
                        jsonArray.Add(baseJsonObject);
                    }
                }
            }
            
            return jsonArray;
        }
        
        var dlNode = htmlNode.SelectSingleNode("./dl[@data-contentful-json-object='true']");
        if (dlNode == null)
        {
            dlNode = htmlNode.SelectSingleNode("./dl");
        }

        if (dlNode == null)
        {
            return new JObject();
        }

        return ParseDlAsObject(dlNode);
    }

    private static JObject ParseToRichText(HtmlNode htmlNode)
    {
        var htmlToRichTextConverter = new HtmlToRichTextConverter();
        var richText = htmlToRichTextConverter.ToRichText(htmlNode.InnerHtml);
        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        var richTextValue = JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
        return richTextValue;
    }

    private static JToken ParseDlAsObject(HtmlNode dlNode)
    {
        var obj = new JObject();

        var ddNodes = dlNode.SelectNodes("./dd[@data-json-key]");
        if (ddNodes == null)
        {
            return obj;
        } 

        foreach (var dd in ddNodes)
        {
            var key = dd.GetAttributeValue("data-json-key", "");
            var valueToken = ParseValueFromNode(dd);
            obj[key] = valueToken;
        }

        return obj;
    }

    private static JToken ParseValueFromNode(HtmlNode node)
    {
        var dlChild = node.SelectSingleNode("./dl[@data-contentful-json-object='true']")
                      ?? node.SelectSingleNode("./dl");

        if (dlChild != null)
        {
            return ParseDlAsObject(dlChild);
        }

        var ulChild = node.SelectSingleNode("./ul");
        if (ulChild != null)
        {
            return ParseUlAsArray(ulChild);
        }

        var textValue = System.Web.HttpUtility.HtmlDecode(node.InnerText.Trim());
        return JValue.FromObject(textValue);
    }

    private static JToken ParseUlAsArray(HtmlNode ulNode)
    {
        var array = new JArray();
        var liNodes = ulNode.SelectNodes("./li");
        if (liNodes != null)
        {
            foreach (var li in liNodes)
            {
                var dlChild = li.SelectSingleNode("./dl[@data-contentful-json-object='true']")
                              ?? li.SelectSingleNode("./dl");
                if (dlChild != null)
                {
                    array.Add(ParseDlAsObject(dlChild));
                    continue;
                }

                var ulChild = li.SelectSingleNode("./ul");
                if (ulChild != null)
                {
                    array.Add(ParseUlAsArray(ulChild));
                    continue;
                }

                var textValue = System.Web.HttpUtility.HtmlDecode(li.InnerText.Trim());
                array.Add(JValue.FromObject(textValue));
            }
        }

        return array;
    }

    // Helper method to build a proper JSON path from data-path attribute
    private static string BuildJsonPathFromDataPath(string dataPath, string field)
    {
        var segments = dataPath.Split('.');
        if (segments.Length < 2) return null;
        
        // Remove the first segment (field name) and array index from path
        var propertyPath = new List<string>();
        
        for (int i = 1; i < segments.Length; i++)
        {
            string segment = segments[i];
            
            // Handle array notation [index]
            if (segment.Contains("[") && segment.Contains("]"))
            {
                // Skip array index notation as we're dealing with the full object
                continue;
            }
            
            propertyPath.Add(segment);
        }
        
        // Add the field name to access the correct property
        propertyPath.Add(field);
        
        return string.Join(".", propertyPath);
    }
    
    // Helper method to get a token by path
    private static bool TryGetTokenByPath(JToken token, string path, out JToken result)
    {
        result = null;
        if (string.IsNullOrEmpty(path)) return false;
        
        try
        {
            var pathSegments = path.Split('.');
            JToken current = token;
            
            foreach (var segment in pathSegments)
            {
                if (current is JObject jObject)
                {
                    if (!jObject.TryGetValue(segment, out current))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            
            result = current;
            return true;
        }
        catch
        {
            return false;
        }
    }
}