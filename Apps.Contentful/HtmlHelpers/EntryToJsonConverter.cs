using System.Web;
using Apps.Contentful.HtmlHelpers.Constants;
using Apps.Contentful.Models;
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
    public static void ToJson(Entry<object> entry, HtmlNode html, string locale)
    {
        var entryFields = (JObject)entry.Fields;

        var elements = html.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element).ToList();
        elements.ForEach(x => UpdateEntryFieldFromHtml(x, entryFields, locale));
    }

    private static void UpdateEntryFieldFromHtml(HtmlNode htmlNode, JObject entryFields, string locale)
    {
        var fieldId = htmlNode.Attributes[ConvertConstants.FieldIdAttribute].Value;
        var fieldType = htmlNode.Attributes[ConvertConstants.FieldTypeAttribute].Value;

        switch (fieldType)
        {
            case "Integer":
                var intValue = Convert.ToInt32(htmlNode.InnerText);
                entryFields[fieldId][locale] = intValue;
                break;
            case "Number":
                var decimalValue = Convert.ToDecimal(htmlNode.InnerText);

                if (decimalValue == Decimal.Floor(decimalValue))
                {
                    entryFields[fieldId][locale] = Decimal.ToInt64(decimalValue);
                    break;
                }
               
                entryFields[fieldId][locale] = decimalValue;

                break;
            case "Symbol" or "Text":
                entryFields[fieldId][locale] = HttpUtility.HtmlDecode(htmlNode.InnerText);
                break;
            case "Object" or "Location":
                var jsonValue = htmlNode.Attributes["data-contentful-json-value"].Value;
                var jsonObject = JToken.Parse(HttpUtility.HtmlDecode(jsonValue));
                entryFields[fieldId][locale] = jsonObject;
                break;
            case "Boolean":
                var boolValue = Convert.ToBoolean(htmlNode.Attributes["data-contentful-bool-value"].Value);
                entryFields[fieldId][locale] = boolValue;
                break;
            case "RichText":
                var htmlToRichTextConverter = new HtmlToRichTextConverter();
                var richText = htmlToRichTextConverter.ToRichText(htmlNode.InnerHtml);
                var serializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };
                entryFields[fieldId][locale] = JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
                break;
            case "Link":
                var linkType = htmlNode.Attributes["data-contentful-link-type"].Value;
                var id = htmlNode.Attributes["data-contentful-link-id"].Value;
                var linkData = new
                {
                    sys = new
                    {
                        type = "Link",
                        linkType,
                        id
                    }
                };
                entryFields[fieldId][locale] = JObject.Parse(JsonConvert.SerializeObject(linkData));
                break;
            case "Array":
                if (htmlNode.Attributes.Any(a => a.Name == "data-contentful-link-type"))
                {
                    linkType = htmlNode.Attributes["data-contentful-link-type"].Value;
                    var itemIds = htmlNode.Attributes["data-contentful-link-ids"].Value.Split(",");
                    var arrayData = itemIds.Select(id => new
                    {
                        sys = new
                        {
                            type = "Link",
                            linkType,
                            id
                        }
                    });
                    entryFields[fieldId][locale] =
                        JArray.Parse(JsonConvert.SerializeObject(arrayData));
                }
                else
                {
                    var arrayContent = htmlNode
                        .Descendants()
                        .Where(x => x.Name == HtmlConstants.Li)
                        .Select(x => HttpUtility.HtmlDecode(x.InnerText));

                    entryFields[fieldId][locale] = JArray.FromObject(arrayContent);
                }

                break;
        }
    }
}