using Apps.Contentful.HtmlHelpers.Constants;
using Apps.Contentful.Models;
using Contentful.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.HtmlHelpers;

public static class EntryToHtmlConverter
{
    public static string ToHtml(List<EntryContentDto> entriesContent, string locale, string spaceId)
    {
        var (doc, bodyNode) = PrepareEmptyHtmlDocument();

        entriesContent.ForEach(x =>
        {
            try
            {
                var entryNode = doc.CreateElement(HtmlConstants.Div);
                entryNode.SetAttributeValue(ConvertConstants.EntryIdAttribute, x.Id);

                bodyNode.AppendChild(entryNode);
                x.ContentTypeFields.ToList()
                    .ForEach(y => MapFieldToHtml(y, x.EntryFields, locale, spaceId, doc, entryNode));
            }
            catch(Exception ex)
            {
                throw new Exception($"Error converting Contentful entry {x.Id} to HTML for locale {locale} (space {spaceId}) | {JsonConvert.SerializeObject(x.EntryFields)}");
            }
        });

        return doc.DocumentNode.OuterHtml;
    }

    private static void MapFieldToHtml(Field field, JObject entryFields, string locale, string spaceId,
        HtmlDocument doc,
        HtmlNode bodyNode)
    {
        if (!entryFields.TryGetValue(field.Id, out var entryField))
            return;

        if (entryField[locale] is null || entryField[locale]!.Type == JTokenType.Null)
            return;
        
        var node = field.Type switch
        {
            "Integer" or "Number" or "Symbol" or "Text" or "Date" => ConvertPrimitivesToHtml(bodyNode, doc, field,
                entryField, locale),
            "Object" or "Location" => ConvertObjectToHtml(doc, field, entryField, locale),
            "Boolean" => ConvertBooleanToHtml(doc, field, entryField, locale),
            "RichText" => ConvertRichTextToHtml(doc, field, entryField, locale, spaceId),
            "Array" => ConvertArrayToHtml(doc, field, entryField, locale),
            "Link" => ConvertLinkToHtml(doc, field, entryField, locale),
            _ => null
        };

        if (node is not null)
            bodyNode.AppendChild(node);
    }

    private static HtmlNode? ConvertLinkToHtml(HtmlDocument doc, Field field, JToken entryField, string locale)
    {
        var linkData = entryField[locale]?["sys"];

        if (linkData is null)
            return default;

        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-link-type", linkData["linkType"].ToString() },
            { "data-contentful-link-id", linkData["id"].ToString() }
        };
        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private static HtmlNode? ConvertArrayToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var itemType = field.Items.Type;
        var arrayItems = (JArray?)entryField?[locale];

        if (arrayItems is null)
            return default;

        if (itemType != "Link")
            return WrapFieldInDiv(doc, field.Type, field.Id, arrayItems.ToString());

        var itemIds = string.Join(",", arrayItems.Select(i => i["sys"]["id"]));
        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-link-type", field.Items.LinkType },
            { "data-contentful-link-ids", itemIds }
        };
        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private static HtmlNode? ConvertRichTextToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale, string spaceId)
    {
        var content = (JArray?)entryField[locale]?["content"];

        if (content is null)
            return default;

        var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
        var fieldContent = richTextToHtmlConverter.ToHtml();

        return WrapFieldInDiv(doc, field.Type, field.Id, fieldContent);
    }

    private static HtmlNode? ConvertBooleanToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var boolValue = (bool?)entryField?[locale];

        if (boolValue is null)
            return default;

        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-bool-value", boolValue.ToString() }
        };
        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private static HtmlNode? ConvertObjectToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var jsonValue = entryField[locale]?.ToString();

        if (jsonValue is null)
            return default;

        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-json-value", jsonValue }
        };

        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private static HtmlNode? ConvertPrimitivesToHtml(HtmlNode bodyNode, HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var fieldContent = entryField[locale]?.ToString();

        if (fieldContent is null)
            return default;
        
        return WrapFieldInDiv(doc, field.Type, field.Id, fieldContent);
    }

    private static HtmlNode WrapFieldInDiv(HtmlDocument doc, string fieldType, string fieldId, string fieldContent = "",
        Dictionary<string, string>? additionalAttributes = null)
    {
        var node = doc.CreateElement(HtmlConstants.Div);
        node.SetAttributeValue(ConvertConstants.FieldTypeAttribute, fieldType);
        node.SetAttributeValue(ConvertConstants.FieldIdAttribute, fieldId);
        if (additionalAttributes != null)
            additionalAttributes.ToList().ForEach(x => node.SetAttributeValue(x.Key, x.Value));

        node.InnerHtml = fieldContent;
        return node;
    }

    private static (HtmlDocument document, HtmlNode bodyNode) PrepareEmptyHtmlDocument()
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement(HtmlConstants.Html);
        htmlDoc.DocumentNode.AppendChild(htmlNode);
        htmlNode.AppendChild(htmlDoc.CreateElement(HtmlConstants.Head));

        var bodyNode = htmlDoc.CreateElement(HtmlConstants.Body);
        htmlNode.AppendChild(bodyNode);

        return (htmlDoc, bodyNode);
    }
}