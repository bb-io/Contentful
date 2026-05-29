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

    public static MainEntryDto? GetMainEntryInfo(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var entryIdNode = doc.DocumentNode
            .SelectSingleNode("//meta[@name='blackbird-entry-id']");

        var localeNode = doc.DocumentNode
            .SelectSingleNode("//meta[@name='blackbird-locale']");

        return new MainEntryDto
        {
            EntryId = entryIdNode?.GetAttributeValue("content", null),
            Locale = localeNode?.GetAttributeValue("content", null)
        };
    }

    public static void ToJson(Entry<object> entry, HtmlNode html, string locale, ContentType contentType, bool doNotUpdateReferenceFields)
    {
        var entryFields = entry.Fields as JObject ?? new JObject();
        entry.Fields = entryFields;

        var validFieldNodes = html.Descendants()
            .Where(node => node.NodeType == HtmlNodeType.Element &&
                           node.Attributes.Contains(ConvertConstants.FieldIdAttribute) &&
                           node.Attributes.Contains(ConvertConstants.FieldTypeAttribute))
            .ToList();

        foreach (var fieldNode in validFieldNodes)
        {
            var fieldId = fieldNode.Attributes[ConvertConstants.FieldIdAttribute].Value;
            var fieldType = fieldNode.Attributes[ConvertConstants.FieldTypeAttribute].Value;

            if (string.IsNullOrWhiteSpace(fieldId) || string.IsNullOrWhiteSpace(fieldType))
                continue;
            
            UpdateEntryFieldFromHtml(fieldNode, entryFields, locale, contentType, doNotUpdateReferenceFields);
        }
    }

    private static void UpdateEntryFieldFromHtml(HtmlNode htmlNode, JObject entryFields, string locale, ContentType contentType, bool doNotUpdateReferenceFields)
    {
        var fieldId = htmlNode.Attributes[ConvertConstants.FieldIdAttribute].Value;
        var fieldType = htmlNode.Attributes[ConvertConstants.FieldTypeAttribute].Value;

        if (doNotUpdateReferenceFields == true)
        {
            var field = contentType.Fields.FirstOrDefault(x => x.Id == fieldId);
            if (field != null)
            {
                var isRef = field.Type == "Link" ||
                            (field.Type == "Array" && field.Items != null && field.Items.LinkType == "Entry");
                if (isRef) return;
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
                var raw = htmlNode.InnerText?.Trim();
                if (!int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var intValue))
                    throw new FieldConversionException(fieldId, raw ?? "", "integer");
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
                SetEntryFieldValue(fieldId, GetPrimitiveTextValue(htmlNode));
                break;
            case "Text":
                SetEntryFieldValue(fieldId, GetPrimitiveTextValue(htmlNode));
                break;
            case "Object":
                JToken? existingLocaleData = null;
    
                if (entryFields.TryGetValue(fieldId, out var existingFieldProp) && existingFieldProp is JObject fieldJObj)
                {
                    if (fieldJObj.TryGetValue(locale, out var localeJToken))
                        existingLocaleData = localeJToken;
                }

                var parsedObject = ParseJsonObjectFromHtmlNode(htmlNode, existingLocaleData);
                SetEntryFieldValue(fieldId, parsedObject);
                break;
            case "Location":
                var jsonValue = htmlNode.Attributes["data-contentful-json-value"].Value;
                var jsonObject = ParseJsonAttribute(jsonValue);
                SetEntryFieldValue(fieldId, jsonObject);
                break;
            case "Boolean":
                var rawBool = htmlNode.Attributes["data-contentful-bool-value"]?.Value?.Trim();
                bool parsed;
                if (bool.TryParse(rawBool, out var b)) parsed = true;
                else if (rawBool is "1" or "yes" or "Yes" or "YES") { b = true; parsed = true; }
                else if (rawBool is "0" or "no" or "No" or "NO") { b = false; parsed = true; }
                else parsed = false;

                if (!parsed) throw new FieldConversionException(fieldId, rawBool ?? "", "boolean (true/false/1/0)");
                SetEntryFieldValue(fieldId, b);
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

    private static string GetPrimitiveTextValue(HtmlNode htmlNode)
    {
        var isHtmlContent = htmlNode.Attributes["data-contentful-html"]?.Value
            ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        var value = isHtmlContent ? htmlNode.InnerHtml : htmlNode.InnerText;
        return HttpUtility.HtmlDecode(value);
    }

    private static JToken ParseJsonObjectFromHtmlNode(HtmlNode htmlNode, JToken? existingTargetData = null)
    {
        var richTextAttribute = htmlNode.Attributes["data-rich-text"];
        if (richTextAttribute != null && richTextAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return ParseToRichText(htmlNode);
        }

        var arrayJsonAttribute = htmlNode.Attributes["data-array-json-object"];
        if (arrayJsonAttribute != null && arrayJsonAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return ParseArrayJsonObject(htmlNode, existingTargetData as JArray);
        }

        var nestedRtAttribute = htmlNode.Attributes["data-nested-rt-json-object"];
        if (nestedRtAttribute != null && nestedRtAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return ParseNestedRtJsonObject(htmlNode, existingTargetData as JObject);
        }

        JObject baseObject = new JObject();
        
        var jsonValueAttribute = htmlNode.Attributes[ConvertConstants.JsonValue]?.Value;
        if (!string.IsNullOrEmpty(jsonValueAttribute))
        {
            try
            {
                baseObject = ParseJsonAttribute(jsonValueAttribute) as JObject ?? new JObject();
            }
            catch
            {
                // We'll fallback to empty object if parsing fails
            }
        }

        if (existingTargetData is JObject existingObj && existingObj.HasValues)
        {
            baseObject.Merge(existingObj, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Merge,
                MergeNullValueHandling = MergeNullValueHandling.Ignore
            });
        }

        var dlNode = htmlNode.SelectSingleNode("./dl[@data-contentful-json-object='true']") 
                  ?? htmlNode.SelectSingleNode("./dl");

        return dlNode == null 
            ? baseObject 
            : ParseDlAsObject(dlNode, baseObject);
    }
    
    private static JToken ParseArrayJsonObject(HtmlNode htmlNode, JArray? existingTargetArray = null)
    {
        var jsonArray = new JArray();
        var customFieldNodes = htmlNode.SelectNodes(".//div[@data-field='customField']");
        if (customFieldNodes != null)
        {
            for (int i = 0; i < customFieldNodes.Count; i++)
            {
                var customFieldNode = customFieldNodes[i];
                var jsonValue = customFieldNode.Attributes["data-contentful-json-value"]?.Value;
        
                if (jsonValue != null)
                {
                    JObject baseJsonObject;
                    if (existingTargetArray != null && i < existingTargetArray.Count && existingTargetArray[i] is JObject existingItem)
                        baseJsonObject = (JObject)existingItem.DeepClone();
                    else
                        baseJsonObject = (JObject)ParseJsonAttribute(jsonValue);
                    
                    var childElements = customFieldNode.SelectNodes(".//div[@data-path]");
                    if (childElements != null)
                    {
                        foreach (var childElement in childElements)
                        {
                            var path = childElement.Attributes["data-path"]?.Value;
                            if (path != null)
                            {
                                var token = SelectArrayItemToken(baseJsonObject, path);

                                if (token != null)
                                {
                                    var isRichText = childElement.Attributes["data-rich-text"]?.Value
                                        ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                                    if (isRichText)
                                    {
                                        var richTextJObject = ParseToRichText(childElement);
                                        token.Replace(richTextJObject);
                                    }
                                    else
                                    {
                                        var newValue = childElement.InnerText.Trim();
                                        token.Replace(new JValue(newValue));
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

    private static JToken ParseNestedRtJsonObject(HtmlNode htmlNode, JObject? existingTargetObject = null)
    {
        var jsonValueAttribute = htmlNode.Attributes[ConvertConstants.JsonValue]?.Value;

        JObject baseJsonObject;
        if (existingTargetObject != null && existingTargetObject.HasValues)
            baseJsonObject = (JObject)existingTargetObject.DeepClone();
        else if (!string.IsNullOrEmpty(jsonValueAttribute))
            baseJsonObject = ParseJsonAttribute(jsonValueAttribute) as JObject ?? new JObject();
        else
            baseJsonObject = new JObject();

        var childDivs = htmlNode.SelectNodes("./div[@data-field]");
        if (childDivs != null)
        {
            foreach (var childDiv in childDivs)
            {
                var fieldName = childDiv.Attributes["data-field"]?.Value;
                if (string.IsNullOrEmpty(fieldName))
                    continue;

                var isRichText = childDiv.Attributes["data-rich-text"]?.Value
                    ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
                var isArrayJson = childDiv.Attributes["data-array-json-object"]?.Value
                    ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                if (isRichText)
                    baseJsonObject[fieldName] = ParseToRichText(childDiv);
                else if (isArrayJson)
                    baseJsonObject[fieldName] = ParseArrayJsonObject(childDiv, baseJsonObject[fieldName] as JArray);
            }
        }

        return baseJsonObject;
    }

    private static JToken ParseJsonAttribute(string jsonValue)
    {
        var decodedJson = HttpUtility.HtmlDecode(jsonValue).Trim();

        try
        {
            return JToken.Parse(decodedJson);
        }
        catch (JsonReaderException)
        {
            var recoveredJson = TryRecoverJsonWithTrailingQuote(decodedJson);
            if (recoveredJson != null)
                return JToken.Parse(recoveredJson);

            throw;
        }
    }

    private static string? TryRecoverJsonWithTrailingQuote(string value)
    {
        var endIndex = FindJsonTokenEnd(value);
        if (endIndex == null || endIndex.Value >= value.Length - 1)
            return null;

        var suffix = value[(endIndex.Value + 1)..].Trim();
        if (suffix.Length == 0 || suffix.Any(x => x != '"'))
            return null;

        return value[..(endIndex.Value + 1)];
    }

    private static int? FindJsonTokenEnd(string value)
    {
        var startIndex = 0;
        while (startIndex < value.Length && char.IsWhiteSpace(value[startIndex]))
            startIndex++;

        if (startIndex >= value.Length)
            return null;

        if (value[startIndex] is not ('{' or '['))
            return null;

        var expectedClosings = new Stack<char>();
        var inString = false;
        var escaped = false;

        for (var i = startIndex; i < value.Length; i++)
        {
            var current = value[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '"')
                    inString = false;

                continue;
            }

            if (current == '"')
            {
                inString = true;
                continue;
            }

            if (current == '{')
            {
                expectedClosings.Push('}');
                continue;
            }

            if (current == '[')
            {
                expectedClosings.Push(']');
                continue;
            }

            if (current is '}' or ']')
            {
                if (expectedClosings.Count == 0 || expectedClosings.Pop() != current)
                    return null;

                if (expectedClosings.Count == 0)
                    return i;
            }
        }

        return null;
    }

    private static JToken? SelectArrayItemToken(JObject baseJsonObject, string path)
    {
        var splitPath = path.Split('.');

        for (var skip = 0; skip < splitPath.Length; skip++)
        {
            var candidatePath = string.Join(".", splitPath.Skip(skip));
            if (!string.IsNullOrWhiteSpace(candidatePath))
            {
                var token = baseJsonObject.SelectToken(candidatePath);
                if (token != null)
                    return token;
            }
        }

        return null;
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

    private static JObject ParseDlAsObject(HtmlNode dlNode, JObject? baseTemplate)
    {
        var resultObject = baseTemplate != null ? (JObject)baseTemplate.DeepClone() : new JObject();

        var propertyNodes = dlNode.ChildNodes
            .Where(n => n.Name == "dd" && n.Attributes.Contains("data-json-key"));

        foreach (var ddNode in propertyNodes)
        {
            var jsonKey = ddNode.Attributes["data-json-key"].Value;
            if (string.IsNullOrEmpty(jsonKey)) 
                continue;

            var innerDl = ddNode.SelectSingleNode("./dl");
            var innerUl = ddNode.SelectSingleNode("./ul");

            if (innerDl != null)
            {
                var subTemplate = resultObject[jsonKey] as JObject;
                resultObject[jsonKey] = ParseDlAsObject(innerDl, subTemplate);
            }
            else if (innerUl != null)
            {
                var listItems = innerUl.SelectNodes("./li");
                var targetArray = resultObject[jsonKey] as JArray ?? [];
                var newArray = new JArray();

                if (listItems != null)
                {
                    for (int i = 0; i < listItems.Count; i++)
                    {
                        var liNode = listItems[i];
                        var itemDl = liNode.SelectSingleNode("./dl");

                        if (itemDl != null)
                        {
                            var itemTemplate = targetArray.Count > i ? targetArray[i] as JObject : null;
                            newArray.Add(ParseDlAsObject(itemDl, itemTemplate));
                        }
                        else
                        {
                            var decodedText = HttpUtility.HtmlDecode(liNode.InnerText.Trim());
                            newArray.Add(new JValue(decodedText));
                        }
                    }
                }
                resultObject[jsonKey] = newArray;
            }
            else
            {
                var decodedValue = HttpUtility.HtmlDecode(ddNode.InnerText.Trim());
                resultObject[jsonKey] = new JValue(decodedValue);
            }
        }
        
        return resultObject;
    }

    private static JToken ParseValueFromNode(HtmlNode node, JToken? baseTemplate = null)
    {
        var dlChild = node.SelectSingleNode("./dl[@data-contentful-json-object='true']")
                      ?? node.SelectSingleNode("./dl");

        if (dlChild != null)
        {
            return ParseDlAsObject(dlChild, baseTemplate as JObject);
        }

        var ulChild = node.SelectSingleNode("./ul");
        if (ulChild != null)
        {
            return ParseUlAsArray(ulChild, baseTemplate as JArray);
        }

        var textValue = HttpUtility.HtmlDecode(node.InnerText.Trim());

        if (bool.TryParse(textValue, out var boolValue))
            return JValue.FromObject(boolValue);
        
        if (decimal.TryParse(textValue, out var decimalValue))
            return JValue.FromObject(decimalValue);

        return JValue.FromObject(textValue);
    }

    private static JArray ParseUlAsArray(HtmlNode ulNode, JArray? baseTemplateArray = null)
    {
        var array = new JArray();
        var liNodes = ulNode.SelectNodes("./li");

        if (liNodes == null) 
            return array;
        
        for (int i = 0; i < liNodes.Count; i++)
        {
            var liNode = liNodes[i];
        
            JToken? itemTemplate = null;
            if (baseTemplateArray != null && i < baseTemplateArray.Count)
                itemTemplate = baseTemplateArray[i];

            var parsedItem = ParseValueFromNode(liNode, itemTemplate);
            array.Add(parsedItem);
        }

        return array;
    }
}
