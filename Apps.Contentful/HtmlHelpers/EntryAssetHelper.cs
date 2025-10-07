using Apps.Contentful.Api;
using Apps.Contentful.Dtos;
using Contentful.Core.Models.Management;
using HtmlAgilityPack;

namespace Apps.Contentful.HtmlHelpers;

public static class EntryAssetHelper
{
    public static bool UpdateImageTitle(ManagementAsset? asset, string altText, string locale)
    {
        if (asset == null || string.IsNullOrWhiteSpace(altText))
            return false;

        asset.Title ??= new Dictionary<string, string>();

        var currentTitle = asset.Title.TryGetValue(locale, out var existingTitle)
            ? existingTitle
            : null;

        if (currentTitle == altText)
            return false;

        asset.Title[locale] = altText;
        return true;
    }

    public static async Task<IEnumerable<ImageUpdateCandidate>> GetImagesToUpdate(string htmlContent, ContentfulClient client)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        var imgNodes = htmlDoc.DocumentNode.SelectNodes("//img[@data-contentful-link-id]");
        if (imgNodes == null) 
            return Enumerable.Empty<ImageUpdateCandidate>();

        var result = new List<ImageUpdateCandidate>();

        foreach (var imgNode in imgNodes)
        {
            var assetId = imgNode.GetAttributeValue("data-contentful-link-id", null);
            var altText = imgNode.GetAttributeValue("alt", null);

            if (string.IsNullOrEmpty(assetId) || string.IsNullOrEmpty(altText))
                continue;

            var asset = await client.ExecuteWithErrorHandling(() => client.GetAsset(assetId));

            var candidate = new ImageUpdateCandidate(asset, altText);
            result.Add(candidate);
        }

        return result;
    }
}
