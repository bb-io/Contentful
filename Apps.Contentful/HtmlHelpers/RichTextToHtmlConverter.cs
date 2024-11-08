using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.HtmlHelpers;

public class RichTextToHtmlConverter
{
    private readonly JArray _content;
    private readonly string _spaceId;
    
    public RichTextToHtmlConverter(JArray content, string spaceId)
    {
        _content = content;
        _spaceId = spaceId;
    }
    
    public string ToHtml()
    {
        var htmlBuilder = new StringBuilder();
        
        foreach (var item in _content)
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
                return $"<h1>{ConvertContentToHtml(jsonObject["content"])}</h1>";
            case "heading-2":
                return $"<h2>{ConvertContentToHtml(jsonObject["content"])}</h2>";
            case "heading-3":
                return $"<h3>{ConvertContentToHtml(jsonObject["content"])}</h3>";
            case "heading-4":
                return $"<h4>{ConvertContentToHtml(jsonObject["content"])}</h4>";
            case "heading-5":
                return $"<h5>{ConvertContentToHtml(jsonObject["content"])}</h5>";
            case "heading-6":
                return $"<h6>{ConvertContentToHtml(jsonObject["content"])}</h6>";
            case "paragraph":
                return $"<p>{ConvertContentToHtml(jsonObject["content"])}</p>";
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
                return $"<a href=\"{uri}\">{ConvertContentToHtml(jsonObject["content"])}</a>";
            case "asset-hyperlink":
                var assetId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{_spaceId}/assets/{assetId}";
                return $"<a id=\"{nodeType}_{assetId}\" href=\"{uri}\">{ConvertContentToHtml(jsonObject["content"])}</a>";
            case "entry-hyperlink":
                var entryId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{_spaceId}/entries/{entryId}";
                return $"<a id=\"{nodeType}_{entryId}\" href=\"{uri}\">{ConvertContentToHtml(jsonObject["content"])}</a>";
            case "embedded-entry-block" or "embedded-entry-inline":
                entryId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{_spaceId}/entries/{entryId}";
                return $"<a id=\"{nodeType}_{entryId}\" href=\"{uri}\"></a>";
            case "embedded-asset-block":
                assetId = jsonObject["data"]["target"]["sys"]["id"].ToString();
                uri = $"https://app.contentful.com/spaces/{_spaceId}/assets/{assetId}";
                return $"<a id=\"{nodeType}_{assetId}\" href=\"{uri}\">Asset {assetId}</a>";
            default:
                return ConvertContentToHtml(jsonObject["content"]);
        }
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
                    return string.Empty;
                
                htmlBuilder.Append(string.IsNullOrWhiteSpace(textContent) ? "<span></span>" : textContent);
            }
            else
                htmlBuilder.Append(ConvertJsonObjectToHtml((JObject)item));
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