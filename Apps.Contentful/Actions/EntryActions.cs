using System.Net.Mime;
using System.Text;
using Apps.Contentful.HtmlHelpers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Newtonsoft.Json;
using Contentful.Core.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json.Serialization;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Contentful.Actions;

[ActionList]
public class EntryActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public EntryActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    #region Text/Rich text fields

    [Action("Get entry's text/rich text field", Description = "Get the text content of the field of the specified entry. " +
                                                              "Field can be short text, long text or rich text. In all " +
                                                              "cases plain text is returned.")]
    public async Task<GetTextContentResponse> GetTextFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][localeIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        string textContent;

        if (fieldType == "RichText")
        {
            var content = (JArray)field["content"];
            var result = new StringBuilder();
            foreach (var item in content)
            {
                var text = string.Join("", item["content"].Select(c => c["value"]));
                result.Append(text);
            }

            textContent = result.ToString();
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
            textContent = field.ToString();
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type."); 

        return new GetTextContentResponse { TextContent = textContent };
    }
    
    [Action("Get entry's text/rich text field as HTML file", Description = "Get the text content of the field of the " +
                                                                           "specified entry as HTML file. Field can be " +
                                                                           "short text, long text or rich text. In all " +
                                                                           "cases HTML file is returned.")]
    public async Task<FileResponse> GetTextFieldContentAsHtmlFile(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId][localeIdentifier.Locale];
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        string html;

        if (fieldType == "RichText")
        {
            var content = (JArray)field["content"];
            var spaceId = Creds.First(p => p.KeyName == "spaceId").Value;
            var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
            html = richTextToHtmlConverter.ToHtml();
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
            html = $"<p>{field}</p>";
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type."); 

        html = $"<html><body>{html}</body></html>";

        return new FileResponse
        {
            File = new File(Encoding.UTF8.GetBytes(html))
            {
                Name = $"{entryIdentifier.EntryId}_{fieldIdentifier.FieldId}_{localeIdentifier.Locale}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }

    [Action("Set entry's text/rich text field", Description = "Set content of the field of the specified entry. Field " +
                                                              "can be short text, long text or rich text.")]
    public async Task SetTextFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Text")] string text)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;

        if (fieldType == "RichText")
        {
            var html = $"<p>{text}</p>";
            var htmlToRichTextConverter = new HtmlToRichTextConverter();
            var richText = htmlToRichTextConverter.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            fields[fieldIdentifier.FieldId][localeIdentifier.Locale] =
                JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
            fields[fieldIdentifier.FieldId][localeIdentifier.Locale] = text;
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type.");
        
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    [Action("Set entry's text/rich text field from HTML file", Description = "Set content of the field of the specified " +
                                                                             "entry from HTML file. Field can be short " +
                                                                             "text, long text or rich text. For short/long " +
                                                                             "text only the text extracted from HTML is " +
                                                                             "put in the field. For rich text all HTML " +
                                                                             "structure is preserved.")]
    public async Task SetTextFieldContentFromHtml(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] FileRequest input)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fieldType = contentType.Fields.First(f => f.Id == fieldIdentifier.FieldId).Type;
        var html = Encoding.UTF8.GetString(input.File.Bytes);

        if (fieldType == "RichText")
        {
            var htmlToRichTextConverter = new HtmlToRichTextConverter();
            var richText = htmlToRichTextConverter.ToRichText(html);
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            fields[fieldIdentifier.FieldId][localeIdentifier.Locale] =
                JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
            
        }
        else if (fieldType == "Text" || fieldType == "Symbol")
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var text = string.Join("", htmlDocument.DocumentNode.SelectNodes("//text()").Select(node => node.InnerText));
            fields[fieldIdentifier.FieldId][localeIdentifier.Locale] = text;
        }
        else
            throw new Exception("The specified field must be of the short text, long text or rich text type."); 

        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }

    #endregion

    #region Number fields

    [Action("Get entry's number field", Description = "Get entry's number field value by field ID.")]
    public async Task<GetNumberContentResponse> GetNumberFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetNumberContentResponse
        {
            NumberContent = fields[fieldIdentifier.FieldId][localeIdentifier.Locale].ToInt()
        };
    }
    
    [Action("Set entry's number field", Description = "Set entry's number field value by field ID.")]
    public async Task SetNumberFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Number")] int number)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][localeIdentifier.Locale] = number;
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    #endregion

    #region Boolean fields

    [Action("Get entry's boolean field", Description = "Get entry's boolean field by field ID.")]
    public async Task<GetBoolContentResponse> GetBoolFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new GetBoolContentResponse
        {
            BooleanContent = fields[fieldIdentifier.FieldId][localeIdentifier.Locale].ToObject<bool>()
        };
    }

    [Action("Set entry's boolean field", Description = "Set entry's boolean field by field ID.")]
    public async Task SetBoolFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] [Display("Boolean")] bool boolean)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][localeIdentifier.Locale] = boolean;
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    #endregion

    #region Media fields

    [Action("Get entry's media content", Description = "Get entry's media content by field ID.")]
    public async Task<AssetIdentifier> GetMediaFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        return new AssetIdentifier
        {
            AssetId = fields[fieldIdentifier.FieldId][localeIdentifier.Locale]["sys"]["id"].ToString()
        };
    }

    [Action("Set entry's media field", Description = "Set entry's media field by field ID.")]
    public async Task SetMediaFieldContent(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var payload = new
        {
            sys = new
            {
                type = "Link",
                linkType = "Asset",
                id = assetIdentifier.AssetId
            }
        };
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        fields[fieldIdentifier.FieldId][localeIdentifier.Locale] = JObject.Parse(JsonConvert.SerializeObject(payload));
        await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
    }
    
    #endregion

    #region Entries

    [Action("List entries", Description = "List all entries with specified content model.")]
    public async Task<ListEntriesResponse> ListEntries([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var queryString = $"?content_type={contentModelIdentifier.ContentModelId}";
        var entries = await client.GetEntriesCollection<Entry<dynamic>>(queryString);
        return new ListEntriesResponse
        {
            Entries = entries.Select(e => new EntryIdentifier { EntryId = e.SystemProperties.Id } )
        };
    }
    
    [Action("Add new entry", Description = "Add new entry with specified content model.")]
    public async Task<EntryIdentifier> AddNewEntry([ActionParameter] ContentModelIdentifier contentModelIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var result = await client.CreateEntry(new Entry<dynamic>(), contentModelIdentifier.ContentModelId);
        return new EntryIdentifier
        {
            EntryId = result.SystemProperties.Id
        };
    }

    [Action("Delete entry", Description = "Delete specified entry.")]
    public async Task DeleteEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.DeleteEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Publish entry", Description = "Publish specified entry.")]
    public async Task PublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.PublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }

    [Action("Unpublish entry", Description = "Unpublish specified entry.")]
    public async Task UnpublishEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        await client.UnpublishEntry(entryIdentifier.EntryId, version: (int)entry.SystemProperties.Version);
    }
    
    [Action("List missing locales for a field", Description = "Retrieve a list of missing locales for a field.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForField(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] FieldIdentifier fieldIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var availableLocales = (await client.GetLocalesCollection()).Select(l => l.Code);
        var field = ((JObject)entry.Fields)[fieldIdentifier.FieldId];
        IEnumerable<string> missingLocales;

        if (field == null)
            missingLocales = availableLocales;
        else
        {
            var fieldDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(field.ToString());
            var presentLocales = fieldDictionary.Select(f => f.Key);
            missingLocales = availableLocales.Except(presentLocales);
        }

        return new ListLocalesResponse { Locales = missingLocales };
    }
    
    [Action("List missing locales for entry", Description = "Retrieve a list of missing locales for specified entry.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForEntry([ActionParameter] EntryIdentifier entryIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        var availableLocales = (await client.GetLocalesCollection()).Select(l => l.Code).ToArray();
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var contentModel = await client.GetContentType(entry.SystemProperties.ContentType.SystemProperties.Id);
        var contentModelLocalizableFields = contentModel.Fields.Where(f => f.Localized);
        var entryFields = (JObject)entry.Fields;
        var missingLocales = new HashSet<string>();
        
        foreach (var field in contentModelLocalizableFields)
        {
            if (!entryFields.TryGetValue(field.Id, out var entryField))
                continue;
            
            foreach (var locale in availableLocales)
            {
                if (!((JObject)entryField).TryGetValue(locale, out _))
                    missingLocales.Add(locale);
            }
        }

        return new ListLocalesResponse { Locales = missingLocales };
    }
    
    [Action("Get entry's localizable fields as HTML file", Description = "Get all localizable fields of specified entry " +
                                                                         "as HTML file.")]
    public async Task<FileResponse> GetEntryLocalizableFieldsAsHtmlFile(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        string WrapFieldInDiv(string fieldType, string fieldId, string fieldContent = "", 
            Dictionary<string, string>? additionalAttributes = null)
        {
            const string contentfulFieldTypeAttribute = "data-contentful-field-type";
            const string contentfulFieldIdAttribute = "data-contentful-field-id";
            var attributesList = $"{contentfulFieldTypeAttribute}=\"{fieldType}\" {contentfulFieldIdAttribute}=\"{fieldId}\"";
            if (additionalAttributes != null)
                attributesList += " " + string.Join(" ", additionalAttributes.Select(a => $"{a.Key}='{a.Value}'"));
            return $"<div {attributesList}>" + $"{fieldContent}</div>";
        }
        
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.GetContentType(contentTypeId);
        var fields = contentType.Fields.Where(f => f.Localized);
        var entryFields = (JObject)entry.Fields;
        var html = new StringBuilder();

        foreach (var field in fields)
        {
            if (!entryFields.TryGetValue(field.Id, out var entryField))
                continue;
                
            switch (field.Type)
            {
                case "Integer" or "Number" or "Symbol" or "Text" or "Date": // Number - decimal; Symbol - short text; Text - long text.
                    var fieldContent = entryField[localeIdentifier.Locale].ToString();
                    var div = WrapFieldInDiv(field.Type, field.Id, fieldContent);
                    html.Append(div);
                    break;
                case "Object" or "Location":
                    var jsonValue = entryField[localeIdentifier.Locale].ToString();
                    var additionalAttributes = new Dictionary<string, string> 
                        { { "data-contentful-json-value", jsonValue } };
                    div = WrapFieldInDiv(field.Type, field.Id, additionalAttributes: additionalAttributes);
                    html.Append(div);
                    break;
                case "Boolean":
                    var boolValue = (bool)entryField[localeIdentifier.Locale];
                    additionalAttributes = new Dictionary<string, string> 
                        { { "data-contentful-bool-value", boolValue.ToString() } };
                    div = WrapFieldInDiv(field.Type, field.Id, additionalAttributes: additionalAttributes);
                    html.Append(div);
                    break;
                case "RichText":
                    var content = (JArray)entryField[localeIdentifier.Locale]["content"];
                    var spaceId = Creds.First(p => p.KeyName == "spaceId").Value;
                    var richTextToHtmlConverter = new RichTextToHtmlConverter(content, spaceId);
                    fieldContent = richTextToHtmlConverter.ToHtml();
                    div = WrapFieldInDiv(field.Type, field.Id, fieldContent);
                    html.Append(div);
                    break;
                case "Link": // media or reference
                    var linkData = entryField[localeIdentifier.Locale]["sys"];
                    additionalAttributes = new Dictionary<string, string>
                    {
                        { "data-contentful-link-type", linkData["linkType"].ToString() },
                        { "data-contentful-link-id", linkData["id"].ToString() }
                    };
                    div = WrapFieldInDiv(field.Type, field.Id, additionalAttributes: additionalAttributes);
                    html.Append(div);
                    break;
                case "Array": // array of links or symbols
                    var itemType = field.Items.Type;
                    var arrayItems = (JArray)entryField[localeIdentifier.Locale];
                    if (itemType == "Link")
                    {
                        var itemIds = string.Join(",", arrayItems.Select(i => i["sys"]["id"]));
                        additionalAttributes = new Dictionary<string, string>
                        {
                            { "data-contentful-link-type", field.Items.LinkType },
                            { "data-contentful-link-ids", itemIds }
                        };
                        div = WrapFieldInDiv(field.Type, field.Id, additionalAttributes: additionalAttributes);
                    }
                    else // in this case itemType is "Symbol" 
                        div = WrapFieldInDiv(field.Type, field.Id, arrayItems.ToString());
                    
                    html.Append(div);
                    break;
            }
        }
        
        var resultHtml = $"<html><body>{html}</body></html>";
        
        return new FileResponse
        {
            File = new File(Encoding.UTF8.GetBytes(resultHtml))
            {
                Name = $"{entryIdentifier.EntryId}_{localeIdentifier.Locale}.html",
                ContentType = MediaTypeNames.Text.Html
            }
        };
    }

    [Action("Set entry's localizable fields from HTML file", Description = "Set all localizable fields of specified entry " +
                                                                           "from HTML file.")]
    public async Task SetEntryLocalizableFieldsFromHtmlFile(
        [ActionParameter] EntryIdentifier entryIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] FileRequest input)
    {
        const string contentfulFieldTypeAttribute = "data-contentful-field-type";
        const string contentfulFieldIdAttribute = "data-contentful-field-id";
        var client = new ContentfulClient(Creds);
        var entry = await client.GetEntry(entryIdentifier.EntryId);
        var fields = (JObject)entry.Fields;
        var html = Encoding.UTF8.GetString(input.File.Bytes);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        var body = htmlDocument.DocumentNode.SelectSingleNode("//body");
        if (body != null)
        {
            var elements = body.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element);

            foreach (var element in elements)
            {
                var fieldId = element.Attributes[contentfulFieldIdAttribute].Value;
                var fieldType = element.Attributes[contentfulFieldTypeAttribute].Value;
                
                switch (fieldType)
                {
                    case "Integer":
                        var intValue = Convert.ToInt32(element.InnerText);
                        fields[fieldId][localeIdentifier.Locale] = intValue;
                        break;
                    case "Number":
                        var decimalValue = Convert.ToDecimal(element.InnerText);
                        fields[fieldId][localeIdentifier.Locale] = decimalValue;
                        break;
                    case "Symbol" or "Text" or "Date":
                        fields[fieldId][localeIdentifier.Locale] = element.InnerText;
                        break;
                    case "Object" or "Location":
                        var jsonValue = element.Attributes["data-contentful-json-value"].Value;
                        var jsonObject = JObject.Parse(jsonValue);
                        fields[fieldId][localeIdentifier.Locale] = jsonObject;
                        break;
                    case "Boolean":
                        var boolValue = Convert.ToBoolean(element.Attributes["data-contentful-bool-value"].Value);
                        fields[fieldId][localeIdentifier.Locale] = boolValue;
                        break;
                    case "RichText":
                        var htmlToRichTextConverter = new HtmlToRichTextConverter();
                        var richText = htmlToRichTextConverter.ToRichText(element.InnerHtml);
                        var serializerSettings = new JsonSerializerSettings();
                        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        serializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        fields[fieldId][localeIdentifier.Locale] =
                            JObject.Parse(JsonConvert.SerializeObject(richText, serializerSettings));
                        break;
                    case "Link":
                        var linkType = element.Attributes["data-contentful-link-type"].Value;
                        var id = element.Attributes["data-contentful-link-id"].Value;
                        var linkData = new
                        {
                            sys = new
                            {
                                type = "Link",
                                linkType,
                                id
                            }
                        };
                        fields[fieldId][localeIdentifier.Locale] = JObject.Parse(JsonConvert.SerializeObject(linkData));
                        break;
                    case "Array":
                        if (element.Attributes.Any(a => a.Name == "data-contentful-link-type"))
                        {
                            linkType = element.Attributes["data-contentful-link-type"].Value;
                            var itemIds = element.Attributes["data-contentful-link-ids"].Value.Split(",");
                            var arrayData = itemIds.Select(id => new
                            {
                                sys = new
                                {
                                    type = "Link",
                                    linkType,
                                    id
                                }
                            });
                            fields[fieldId][localeIdentifier.Locale] = JArray.Parse(JsonConvert.SerializeObject(arrayData));
                        }
                        else
                        {
                            var arrayData = JArray.Parse(element.InnerText);
                            fields[fieldId][localeIdentifier.Locale] = arrayData;
                        }
                        break;
                }
            }
            
            await client.CreateOrUpdateEntry(entry, version: entry.SystemProperties.Version);
        }
    }

    #endregion
}