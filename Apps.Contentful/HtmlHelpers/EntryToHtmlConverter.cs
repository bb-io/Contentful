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
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error converting Contentful entry {x.Id} to HTML for locale {locale} (space {spaceId}); " +
                    $"Exception message: {ex.Message}; Exception type: {ex.GetType()} | {JsonConvert.SerializeObject(x.EntryFields)};");
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
            "Object" => ConvertJsonObjectToHtml(doc, field, entryField, locale),
            "Location" => ConvertLocationObjectToHtml(doc, field, entryField, locale),
            "Boolean" => ConvertBooleanToHtml(doc, field, entryField, locale),
            "RichText" => ConvertRichTextToHtml(doc, field, entryField, locale, spaceId),
            "Array" => ConvertArrayToHtml(doc, field, entryField, locale),
            "Link" => ConvertLinkToHtml(doc, field, entryField, locale),
            _ => null
        };

        if (node is not null)
            bodyNode.AppendChild(node);
    }

    private HtmlNode? ConvertJsonObjectToHtml(HtmlDocument doc, Field field, JToken entryField, string locale)
    {
        var jsonToken = entryField[locale];
        if (jsonToken == null || jsonToken.Type != JTokenType.Object)
            return null;

        // Create a container <div> to hold the entire JSON structure
        // and mark it as a JSON object container.
        var containerNode = doc.CreateElement("div");
        containerNode.SetAttributeValue(ConvertConstants.FieldTypeAttribute, field.Type);
        containerNode.SetAttributeValue(ConvertConstants.FieldIdAttribute, field.Id);
        containerNode.SetAttributeValue("data-contentful-json-object", "true");

        // Convert the JObject to a <dl> definition list
        var dlNode = doc.CreateElement("dl");
        containerNode.AppendChild(dlNode);

        // Recursive function to render JToken as HTML
        HtmlNode RenderToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    // For objects, create a <dl> and for each property create <dt> and <dd> pairs.
                    var innerDl = doc.CreateElement("dl");
                    innerDl.SetAttributeValue("data-contentful-json-object", "true");

                    foreach (var prop in ((JObject)token).Properties())
                    {
                        // Create <dd> for the value
                        var ddNode = doc.CreateElement("dd");
                        ddNode.SetAttributeValue("data-json-key", prop.Name);

                        // If value is a string, encode and place it directly
                        // If it's another object/array, recursively render
                        if (prop.Value.Type == JTokenType.String)
                        {
                            // Only string values are translatable text
                            ddNode.InnerHtml = System.Net.WebUtility.HtmlEncode(prop.Value.ToString());
                        }
                        else
                        {
                            // For non-string values (object/array), recurse
                            var childNode = RenderToken(prop.Value);
                            ddNode.AppendChild(childNode);
                        }

                        innerDl.AppendChild(ddNode);
                    }

                    return innerDl;

                case JTokenType.Array:
                    // For arrays, create a <ul> and each element is a <li>
                    var ulNode = doc.CreateElement("ul");

                    foreach (var item in (JArray)token)
                    {
                        var liNode = doc.CreateElement("li");
                        if (item.Type == JTokenType.String)
                        {
                            liNode.InnerHtml = System.Net.WebUtility.HtmlEncode(item.ToString());
                        }
                        else
                        {
                            // If it's an object or array, recurse
                            var childNode = RenderToken(item);
                            liNode.AppendChild(childNode);
                        }

                        ulNode.AppendChild(liNode);
                    }

                    return ulNode;

                case JTokenType.String:
                    // Just in case we get a string here directly
                    var textNode = doc.CreateElement("span");
                    textNode.InnerHtml = System.Net.WebUtility.HtmlEncode(token.ToString());
                    return textNode;

                default:
                    // For other types (numbers, bool, null), treat them as strings
                    var defaultNode = doc.CreateElement("span");
                    defaultNode.InnerHtml = System.Net.WebUtility.HtmlEncode(token.ToString() ?? "");
                    return defaultNode;
            }
        }

        // Render the main object
        var rootObject = (JObject)jsonToken;
        foreach (var prop in rootObject.Properties())
        {
            var ddNode = doc.CreateElement("dd");
            ddNode.SetAttributeValue("data-json-key", prop.Name);


            if (prop.Value.Type == JTokenType.String)
            {
                ddNode.InnerHtml = System.Net.WebUtility.HtmlEncode(prop.Value.ToString());
            }
            else
            {
                var childNode = RenderToken(prop.Value);
                ddNode.AppendChild(childNode);
            }

            dlNode.AppendChild(ddNode);
        }

        return containerNode;
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
            { "data-contentful-link-id", linkId! },
            { "data-contentful-localized", field.Localized.ToString() }
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

        if (itemType == "Link")
        {
            additionalAttributes.Add("data-contentful-localized", field.Localized.ToString());
        }

        return WrapFieldInDiv(doc, field.Type, field.Id, additionalAttributes: additionalAttributes);
    }

    private HtmlNode? ConvertRichTextToHtml(HtmlDocument doc, Field field, JToken entryField,
        string locale, string spaceId)
    {
        var content = (JArray?)entryField[locale]?["content"];

        if (content is null)
            return default;

        var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
        var fieldContent = richTextToHtmlConverter.ToHtml();

        return WrapFieldInDiv(doc, field.Type, field.Id, fieldContent);
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

    private HtmlNode? ConvertLocationObjectToHtml(HtmlDocument doc, Field field, JToken entryField,
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