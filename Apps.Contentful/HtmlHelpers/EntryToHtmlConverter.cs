using Apps.Contentful.Api;
using Apps.Contentful.HtmlHelpers.Constants;
using Apps.Contentful.Models;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.HtmlHelpers;

public class EntryToHtmlConverter(InvocationContext invocationContext, string? environment)
{
    public string ToHtml(List<EntryContentDto> entriesContent, string locale, string spaceId)
    {
        var entryId = entriesContent.Select(x => x.Id).FirstOrDefault() ?? string.Empty;
        var (doc, bodyNode) = PrepareEmptyHtmlDocument(entryId, locale);

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

    private void MapFieldToHtml(Field field, JObject entryFields, string locale, string spaceId,
        HtmlDocument doc,
        HtmlNode bodyNode)
    {
        if (!entryFields.TryGetValue(field.Id, out var entryField))
            return;

        if (entryField[locale] is null || entryField[locale]!.Type == JTokenType.Null)
            return;
        
        var node = field.Type switch
        {
            "Integer" or "Number" or "Symbol" or "Text" => ConvertPrimitivesToHtml(bodyNode, doc, field,
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

    private HtmlNode? ConvertLinkToHtml(HtmlDocument doc, Field field, JToken entryField, string locale)
    {
        var linkData = entryField[locale]?["sys"];

        if (linkData is null)
            return default;
        
        var linkId = linkData["id"]?.ToString();
        var linkType = linkData["linkType"]?.ToString();
        
        if (linkType == "Asset")
        {
            var client = new ContentfulClient(invocationContext.AuthenticationCredentialsProviders, environment);
            var assetTask = client.GetAsset(linkId!);
            assetTask.Wait();
            
            var asset = assetTask.Result;

            if (asset.Files != null)
            {
                foreach (var fileEntry in asset.Files)
                {
                    var fileLocale = fileEntry.Key;
                    var file = fileEntry.Value;

                    if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        var imageUrl = file.Url;

                        if (imageUrl.StartsWith("//"))
                        {
                            imageUrl = "https:" + imageUrl;
                        }
                        else if (imageUrl.StartsWith("/"))
                        {
                            imageUrl = "https://images.ctfassets.net" + imageUrl;
                        }

                        var imgNode = doc.CreateElement("img");
                        imgNode.SetAttributeValue("src", imageUrl);
                        imgNode.SetAttributeValue(ConvertConstants.FieldTypeAttribute, field.Type);
                        imgNode.SetAttributeValue(ConvertConstants.FieldIdAttribute, field.Id);
                        imgNode.SetAttributeValue("data-contentful-link-type", linkType);
                        imgNode.SetAttributeValue("data-contentful-link-id", linkId);
                    
                        var altText = asset.Title.ContainsKey(fileLocale) ? asset.Title[fileLocale] : "";
                        if (!string.IsNullOrWhiteSpace(altText))
                        {
                            imgNode.SetAttributeValue("alt", altText);
                        }

                        return imgNode;
                    }
                }
            } 
        }

        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-link-type", linkType! },
            { "data-contentful-link-id", linkId! }
        };
        
        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private HtmlNode? ConvertArrayToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var itemType = field.Items.Type;
        var arrayItems = (JArray?)entryField?[locale];

        if (arrayItems is null)
            return default;

        if (itemType != "Link")
            return WrapFieldInList(doc, field.Type, field.Id, arrayItems);

        var itemIds = string.Join(",", arrayItems.Select(i => i["sys"]["id"]));
        var additionalAttributes = new Dictionary<string, string>
        {
            { "data-contentful-link-type", field.Items.LinkType },
            { "data-contentful-link-ids", itemIds }
        };
        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private HtmlNode? ConvertRichTextToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale, string spaceId)
    {
        var content = (JArray?)entryField[locale]?["content"];

        if (content is null)
            return default;
        
        RemoveEmbeddedEntries(content);

        var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
        var fieldContent = richTextToHtmlConverter.ToHtml();

        return WrapFieldInDiv(doc, field.Type, field.Id, fieldContent);
    }
    
    private void RemoveEmbeddedEntries(JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            var nodeType = obj["nodeType"]?.ToString();

            if (nodeType == "embedded-entry-inline" || nodeType == "embedded-entry-block")
            {
                token.Remove();
                return;
            }
        }

        var children = token.Children().ToList();
        foreach (var child in children)
        {
            RemoveEmbeddedEntries(child);
        }
    }

    private HtmlNode? ConvertBooleanToHtml(HtmlDocument doc, Field field, JToken entryField,
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

    private HtmlNode? ConvertObjectToHtml(HtmlDocument doc, Field field, JToken entryField,
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

    private HtmlNode? ConvertPrimitivesToHtml(HtmlNode bodyNode, HtmlDocument doc, Field field, JToken entryField,
        string locale)
    {
        var fieldContent = entryField[locale]?.ToString();

        if (fieldContent is null)
            return default;

        var tagName = HtmlConstants.Div;
        if (string.Equals(field.Id, "title", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field.Name, "title", StringComparison.OrdinalIgnoreCase))
        {
            tagName = "h1";
        }
        else if (string.Equals(field.Id, "subtitle", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(field.Name, "subtitle", StringComparison.OrdinalIgnoreCase))
        {
            tagName = "h2";
        }

        return WrapFieldInDiv(doc, field.Type, field.Id, fieldContent, tagName: tagName);
    }

    private HtmlNode WrapFieldInDiv(HtmlDocument doc, string fieldType, string fieldId, string fieldContent = "",
        Dictionary<string, string>? additionalAttributes = null, string tagName = HtmlConstants.Div)
    {
        var node = doc.CreateElement(tagName);
        node.SetAttributeValue(ConvertConstants.FieldTypeAttribute, fieldType);
        node.SetAttributeValue(ConvertConstants.FieldIdAttribute, fieldId);

        if (additionalAttributes != null)
            foreach (var attr in additionalAttributes)
                node.SetAttributeValue(attr.Key, attr.Value);

        node.InnerHtml = fieldContent;
        return node;
    }   
    
    private HtmlNode WrapFieldInList(HtmlDocument doc, string fieldType, string fieldId, JArray fieldContent,
        Dictionary<string, string>? additionalAttributes = null)
    {
        var node = doc.CreateElement(HtmlConstants.Div);
        node.SetAttributeValue(ConvertConstants.FieldTypeAttribute, fieldType);
        node.SetAttributeValue(ConvertConstants.FieldIdAttribute, fieldId);
        if (additionalAttributes != null)
            additionalAttributes.ToList().ForEach(x => node.SetAttributeValue(x.Key, x.Value));

        var ulNode = doc.CreateElement(HtmlConstants.Ul);

        foreach (var item in fieldContent)
        {
            var liNode = doc.CreateElement(HtmlConstants.Li);
            liNode.InnerHtml = item.ToString();
            
            ulNode.AppendChild(liNode);
        }

        node.AppendChild(ulNode);
        return node;
    }

    private (HtmlDocument document, HtmlNode bodyNode) PrepareEmptyHtmlDocument(string entryId, string locale)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement(HtmlConstants.Html);
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement(HtmlConstants.Head);
        htmlNode.AppendChild(headNode);

        var entryMetaNode = htmlDoc.CreateElement("meta");
        entryMetaNode.SetAttributeValue("name", "blackbird-entry-id");
        entryMetaNode.SetAttributeValue("content", entryId);
        headNode.AppendChild(entryMetaNode);

        var localeMetaNode = htmlDoc.CreateElement("meta");
        localeMetaNode.SetAttributeValue("name", "blackbird-locale");
        localeMetaNode.SetAttributeValue("content", locale);
        headNode.AppendChild(localeMetaNode);

        var bodyNode = htmlDoc.CreateElement(HtmlConstants.Body);
        htmlNode.AppendChild(bodyNode);

        return (htmlDoc, bodyNode);
    }
}