using Apps.Contentful.HtmlHelpers.Constants;
using Contentful.Core.Models.Management;
using HtmlAgilityPack;
using System.Web;
using ContentfulModels = Contentful.Core.Models;

namespace Apps.Contentful.HtmlHelpers;

public class AssetToJsonConverter
{
    private readonly ManagementAsset _asset;
    private readonly string _locale;

    public AssetToJsonConverter(ManagementAsset asset, string locale)
    {
        _asset = asset;
        _locale = locale;
    }

    public void LocalizeAsset(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var assetNode = doc.DocumentNode.SelectSingleNode($"//div[@{ConvertConstants.AssetIdAttribute}='{_asset.SystemProperties.Id}']")
            ?? throw new ArgumentException($"Asset node not found for ID: {_asset.SystemProperties.Id}");

        UpdateAssetFields(assetNode);
    }

    private void UpdateAssetFields(HtmlNode assetNode)
    {
        UpdateTitle(assetNode);
        UpdateDescription(assetNode);
        UpdateFile(assetNode);
    }

    private void UpdateTitle(HtmlNode assetNode)
    {
        var titleNode = assetNode.SelectSingleNode($"./h1[@{ConvertConstants.FieldIdAttribute}='title']");
        if (titleNode != null)
        {
            var title = HttpUtility.HtmlDecode(titleNode.InnerText);
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (_asset.Title == null)
                {
                    _asset.Title = new Dictionary<string, string>();
                }
                _asset.Title[_locale] = title;
            }
        }
    }

    private void UpdateDescription(HtmlNode assetNode)
    {
        var descNode = assetNode.SelectSingleNode($"./div[@{ConvertConstants.FieldIdAttribute}='description']");
        if (descNode != null)
        {
            var description = HttpUtility.HtmlDecode(descNode.InnerHtml);
            if (!string.IsNullOrWhiteSpace(description))
            {
                if (_asset.Description == null)
                {
                    _asset.Description = new Dictionary<string, string>();
                }
                _asset.Description[_locale] = description;
            }
        }
    }

    private void UpdateFile(HtmlNode assetNode)
    {
        var fileNode = assetNode.SelectSingleNode($".//*[@{ConvertConstants.FieldIdAttribute}='file']");
        if (fileNode != null)
        {
            if (_asset.Files == null)
            {
                _asset.Files = new Dictionary<string, ContentfulModels.File>();
            }

            var file = new ContentfulModels.File
            {
                FileName = fileNode.GetAttributeValue("data-filename", string.Empty),
                ContentType = fileNode.GetAttributeValue("data-content-type", string.Empty)
            };

            if (int.TryParse(fileNode.GetAttributeValue("data-size", "0"), out var size) && size > 0)
            {
                file.Details = new ContentfulModels.FileDetails { Size = size };
            }

            if (fileNode.Name == "img")
            {
                if (int.TryParse(fileNode.GetAttributeValue("width", "0"), out var width) &&
                    int.TryParse(fileNode.GetAttributeValue("height", "0"), out var height) &&
                    width > 0 && height > 0)
                {
                    if (file.Details == null)
                    {
                        file.Details = new ContentfulModels.FileDetails();
                    }
                    
                    file.Details.Image = new ContentfulModels.ImageDetails
                    {
                        Width = width,
                        Height = height
                    };
                }

                var src = fileNode.GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrEmpty(src))
                {
                    file.Url = NormalizeUrl(src);
                }
            }
            else if (fileNode.Name == "a")
            {
                var href = fileNode.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(href))
                {
                    file.Url = NormalizeUrl(href);
                }
            }

            _asset.Files[_locale] = file;
        }
    }

    private static string NormalizeUrl(string url)
    {
        if (url.StartsWith("https:") || url.StartsWith("http:"))
        {
            return url.Replace("https:", "").Replace("http:", "");
        }
        return url;
    }
}
