using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models.Management;
using HtmlAgilityPack;
using System.Text;
using Apps.Contentful.HtmlHelpers.Constants;

namespace Apps.Contentful.HtmlHelpers;

public record AssetHtmlVariables(string AssetId, string Locale, string Environment);

public class AssetToHtmlConverter(string? environment)
{
    public static AssetHtmlVariables GetVariablesFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var assetId = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-asset-id']")?.GetAttributeValue("content", string.Empty)
            ?? throw new ArgumentException("Asset ID not found in HTML.");
        var locale = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-locale']")?.GetAttributeValue("content", string.Empty)
            ?? throw new ArgumentException("Locale not found in HTML.");
        var environment = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-environment']")?.GetAttributeValue("content", string.Empty)
            ?? throw new ArgumentException("Environment not found in HTML.");

        return new AssetHtmlVariables(assetId, locale, environment);
    }

    public string GetFileName(ManagementAsset asset, string locale)
    {
        var fileName = asset.SystemProperties.Id;
        
        if (asset.Title != null && asset.Title.TryGetValue(locale, out var title) && !string.IsNullOrWhiteSpace(title))
        {
            fileName = title;
        }
        
        return $"{fileName}.html";
    }

    public string ConvertToHtml(ManagementAsset asset, string locale)
    {
        var (doc, bodyNode) = PrepareEmptyHtmlDocument(asset.SystemProperties.Id, locale);
        
        var assetNode = doc.CreateElement(HtmlConstants.Div);
        assetNode.SetAttributeValue(ConvertConstants.AssetIdAttribute, asset.SystemProperties.Id);
        bodyNode.AppendChild(assetNode);
        
        if (asset.Title != null && asset.Title.TryGetValue(locale, out var title) && !string.IsNullOrWhiteSpace(title))
        {
            var titleNode = doc.CreateElement("h1");
            titleNode.SetAttributeValue(ConvertConstants.FieldIdAttribute, "title");
            titleNode.InnerHtml = title;
            assetNode.AppendChild(titleNode);
        }
        
        if (asset.Description != null && asset.Description.TryGetValue(locale, out var description) && !string.IsNullOrWhiteSpace(description))
        {
            var descNode = doc.CreateElement(HtmlConstants.Div);
            descNode.SetAttributeValue(ConvertConstants.FieldIdAttribute, "description");
            
            if (description.Contains("\n"))
            {
                var paragraphs = description.Split("\n");
                var stringBuilder = new StringBuilder();
                foreach (var paragraph in paragraphs)
                {
                    if (string.IsNullOrEmpty(paragraph))
                    {
                        stringBuilder.AppendLine("<br>");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"<p>{paragraph}</p>");
                    }
                }
                
                stringBuilder.Replace("<p></p>", "<br>");
                descNode.InnerHtml = stringBuilder.ToString();
            }
            else
            {
                descNode.InnerHtml = description;
            }
            
            assetNode.AppendChild(descNode);
        }
        
        if (asset.Files != null && asset.Files.TryGetValue(locale, out var file) && file != null)
        {
            var fileUrl = file.Url;
            if (fileUrl.StartsWith("//"))
            {
                fileUrl = "https:" + fileUrl;
            }
            else if (fileUrl.StartsWith("/"))
            {
                fileUrl = "https://images.ctfassets.net" + fileUrl;
            }
            
            HtmlNode fileNode;
            if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                fileNode = doc.CreateElement("img");
                fileNode.SetAttributeValue("src", fileUrl);
                fileNode.SetAttributeValue("alt", asset.Title?.ContainsKey(locale) == true ? asset.Title[locale] : "");
                if (file.Details?.Image != null)
                {
                    fileNode.SetAttributeValue("width", file.Details.Image.Width.ToString());
                    fileNode.SetAttributeValue("height", file.Details.Image.Height.ToString());
                }
            }
            else
            {
                fileNode = doc.CreateElement("a");
                fileNode.SetAttributeValue("href", fileUrl);
                fileNode.InnerHtml = file.FileName;
                fileNode.SetAttributeValue("download", "");
            }
            
            fileNode.SetAttributeValue(ConvertConstants.FieldIdAttribute, "file");
            fileNode.SetAttributeValue("data-filename", file.FileName);
            fileNode.SetAttributeValue("data-content-type", file.ContentType);
            
            if (file.Details?.Size != null)
            {
                fileNode.SetAttributeValue("data-size", file.Details.Size.ToString());
            }
            
            assetNode.AppendChild(fileNode);
        }
        
        return doc.DocumentNode.OuterHtml;
    }
    
    private (HtmlDocument document, HtmlNode bodyNode) PrepareEmptyHtmlDocument(string assetId, string locale)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement(HtmlConstants.Html);
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement(HtmlConstants.Head);
        htmlNode.AppendChild(headNode);

        var assetMetaNode = htmlDoc.CreateElement("meta");
        assetMetaNode.SetAttributeValue("name", "blackbird-asset-id");
        assetMetaNode.SetAttributeValue("content", assetId);
        headNode.AppendChild(assetMetaNode);

        var localeMetaNode = htmlDoc.CreateElement("meta");
        localeMetaNode.SetAttributeValue("name", "blackbird-locale");
        localeMetaNode.SetAttributeValue("content", locale);
        headNode.AppendChild(localeMetaNode);
        
        if (!string.IsNullOrEmpty(environment))
        {
            var environmentMetaNode = htmlDoc.CreateElement("meta");
            environmentMetaNode.SetAttributeValue("name", "blackbird-environment");
            environmentMetaNode.SetAttributeValue("content", environment);
            headNode.AppendChild(environmentMetaNode);
        }

        var bodyNode = htmlDoc.CreateElement(HtmlConstants.Body);
        htmlNode.AppendChild(bodyNode);

        return (htmlDoc, bodyNode);
    }
}
