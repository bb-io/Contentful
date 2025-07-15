using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.HtmlHelpers;

public class RichTextToHtmlConverter(JArray content, string spaceId)
{
    public string ToHtml()
    {
        var htmlBuilder = new StringBuilder();
        
        foreach (var item in content)
        {
            if (item is JObject jsonObject)
                htmlBuilder.Append(ConvertJsonObjectToHtml(jsonObject));
            else if (item is JValue jsonValue)
                htmlBuilder.Append(JsonConvert.ToString(jsonValue.Value));
        }

        return htmlBuilder.ToString();
    }
    
    private string ConvertJsonObjectToHtml(JObject jsonObject)
    {
        var nodeType = jsonObject["nodeType"].ToString();

        switch (nodeType)
        {
            case "heading-1":
            case "heading-2":
            case "heading-3":
            case "heading-4":
            case "heading-5":
            case "heading-6":
                return ConvertHeadingToHtml(jsonObject, nodeType);
            case "paragraph":
                var content = ConvertContentToHtml(jsonObject["content"]);
                content = content.Replace(@"\n", "<br>");
                return string.IsNullOrWhiteSpace(content) ? "<br>" : $"<p>{content}</p>";
            case "blockquote":
                return $"<blockquote>{ConvertContentToHtml(jsonObject["content"])}</blockquote>";
            case "table":
                return $"<table>{ConvertContentToHtml(jsonObject["content"])}</table>";
            case "table-row":
                return $"<tr>{ConvertContentToHtml(jsonObject["content"])}</tr>";
            case "table-header-cell":
                return $"<th>{ConvertContentToHtml(jsonObject["content"])}</th>";
            case "table-cell":
                return $"<td>{ConvertContentToHtml(jsonObject["content"])}</td>";
            case "unordered-list":
                return $"<ul>{ConvertContentToHtml(jsonObject["content"])}</ul>";
            case "ordered-list":
                return $"<ol>{ConvertContentToHtml(jsonObject["content"])}</ol>";
            case "list-item":
                return $"<li>{ConvertContentToHtml(jsonObject["content"])}</li>";
            case "hr":
                return "<hr />";
            case "hyperlink":
                var uri = jsonObject["data"]["uri"].ToString();
                var hyperlinkContent = ConvertContentToHtml(jsonObject["content"]);
                content = hyperlinkContent.Replace("\n", "<br>");
                return $"<a href=\"{uri}\">{content}</a>";
            case "asset-hyperlink":
                var assetId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{spaceId}/assets/{assetId}";
                return $"<a id=\"{nodeType}_{assetId}\" href=\"{uri}\">{ConvertContentToHtml(jsonObject["content"])}</a>";
            case "entry-hyperlink":
                var entryId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{spaceId}/entries/{entryId}";
                return $"<a id=\"{nodeType}_{entryId}\" href=\"{uri}\">{ConvertContentToHtml(jsonObject["content"])}</a>";
            case "embedded-entry-block" or "embedded-entry-inline":
                if(jsonObject["data"]?["quote"] != null)
                {
                    return $"<blockquote data-custom-quote=\"true\">{ConvertQuoteToHtml(jsonObject)}</blockquote>";
                }
                entryId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{spaceId}/entries/{entryId}";
                return $"<a id=\"{nodeType}_{entryId}\" href=\"{uri}\"></a>";
            case "embedded-asset-block":
                assetId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{spaceId}/assets/{assetId}";
                return $"<a id=\"{nodeType}_{assetId}\" href=\"{uri}\">Asset {assetId}</a>";
            default:
                return ConvertContentToHtml(jsonObject["content"]);
        }
    }

    private string ConvertHeadingToHtml(JObject jsonObject, string nodeType)
    {
        var tagName = nodeType.Replace("heading-", "h");
        var content = ConvertHeadingContentToHtml(jsonObject["content"]);
        return $"<{tagName}>{content}</{tagName}>";
    }

    private string ConvertHeadingContentToHtml(JToken content)
    {
        var htmlBuilder = new StringBuilder();

        foreach (var item in content)
        {
            if (item["nodeType"].ToString() == "text")
            {
                var value = item["value"].ToString();
                value = value.Replace("\n", " ");
                GetMarksHtml(item["marks"], out var openingMarks, out var closingMarks);
                htmlBuilder.Append($"{openingMarks}{value}{closingMarks}");
            }
            else
            {
                htmlBuilder.Append(ConvertJsonObjectToHtml((JObject)item));
            }
        }

        return htmlBuilder.ToString();
    }

    private string ConvertQuoteToHtml(JToken quoteToken)
    {
        var htmlBuilder = new StringBuilder();
        var excludedFieldPathes = new[] { "target.sys", "nodeType" };
        foreach (var property in quoteToken.Children<JProperty>())
        {
            if (property.Value.Type == JTokenType.String)
            {
                string value = property.Value.ToString();
                if (!string.IsNullOrEmpty(value) && !excludedFieldPathes.Any(property.Path.Contains))
                {
                    htmlBuilder.Append($"<div data-field=\"{property.Name}\" data-path=\"{property.Path}\">{value}</div>");
                }
            }
            if (property.Value.Type == JTokenType.Object)
            {
                var innerHtml = ConvertQuoteToHtml(property.Value);
                if (!string.IsNullOrEmpty(innerHtml) && !excludedFieldPathes.Any(property.Path.Contains))
                {
                    htmlBuilder.Append($"<div data-field=\"{property.Name}\" data-path=\"{property.Path}\">{innerHtml}</div>");
                }
            }
            if(property.Value.Type == JTokenType.Array)
            {
                foreach (var item in property.Value.Children())
                {
                    var innerHtml = ConvertQuoteToHtml(item);
                    if (!string.IsNullOrEmpty(innerHtml) && !excludedFieldPathes.Any(property.Path.Contains))
                    {
                        htmlBuilder.Append($"<div data-field=\"{property.Name}\" data-path=\"{property.Path}\">{innerHtml}</div>");
                    }
                }
            }
        }
        
        return htmlBuilder.ToString();
    }

    private string ConvertContentToHtml(JToken content)
    {
        var htmlBuilder = new StringBuilder();

        foreach (var item in content)
        {
            if (item["nodeType"].ToString() == "text")
            {
                var value = item["value"].ToString();
                GetMarksHtml(item["marks"], out var openingMarks, out var closingMarks);

                var textContent = $"{openingMarks}{value}{closingMarks}";
                if (content.Count() == 1 && string.IsNullOrWhiteSpace(textContent))
                {
                    return string.Empty;
                }
                
                htmlBuilder.Append(string.IsNullOrWhiteSpace(textContent) ? "<span></span>" : textContent);
            }
            else
            {
                htmlBuilder.Append(ConvertJsonObjectToHtml((JObject)item));
            }
        }

        return htmlBuilder.ToString();
    }

    private void GetMarksHtml(JToken marks, out string openingMarks, out string closingMarks)
    {
        var openingMarksBuilder = new StringBuilder();
        var closingMarksBuilder = new StringBuilder();

        foreach (var mark in marks)
        {
            var markType = mark["type"].ToString();
            switch (markType)
            {
                case "bold":
                    openingMarksBuilder.Append("<strong>");
                    closingMarksBuilder.Insert(0, "</strong>");
                    break;
                case "italic":
                    openingMarksBuilder.Append("<i>");
                    closingMarksBuilder.Insert(0, "</i>");
                    break;
                case "underline":
                    openingMarksBuilder.Append("<u>");
                    closingMarksBuilder.Insert(0, "</u>");
                    break;
                case "code":
                    openingMarksBuilder.Append("<code>");
                    closingMarksBuilder.Insert(0, "</code>");
                    break;
                case "superscript":
                    openingMarksBuilder.Append("<sup>");
                    closingMarksBuilder.Insert(0, "</sup>");
                    break;
                case "subscript":
                    openingMarksBuilder.Append("<sub>");
                    closingMarksBuilder.Insert(0, "</sub>");
                    break;
            }
        }

        openingMarks = openingMarksBuilder.ToString();
        closingMarks = closingMarksBuilder.ToString();
    }
}