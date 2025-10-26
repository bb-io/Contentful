using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Newtonsoft.Json;

namespace Apps.Contentful.Models.Entities;

public class AssetEntity
{
    [Display("Asset ID")]
    public string ContentId { get; set; }

    [Display("Tag IDs")]
    public IEnumerable<string> TagIds { get; set; }

    [Display("Title")]
    public string? Title { get; set; }

    [Display("Description")]
    public string? Description { get; set; }

    [Display("File name")]
    public string? FileName { get; set; }

    [Display("Content type")]
    public string? ContentType { get; set; }

    [Display("File size")]
    public long? FileSize { get; set; }

    [Display("Created at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime? UpdatedAt { get; set; }

    [Display("Updated by (user ID)")]
    public string? UpdatedBy { get; set; }

    public int Version { get; set; }

    public AssetEntity(Asset asset)
    {
        ContentId = asset.SystemProperties.Id;
        TagIds = asset.Metadata?.Tags.Select(x => x.Sys.Id) ?? Enumerable.Empty<string>();
        CreatedAt = asset.SystemProperties.CreatedAt;
        UpdatedAt = asset.SystemProperties.UpdatedAt;
        Version = asset.SystemProperties.Version ?? default;
        UpdatedBy = asset.SystemProperties.UpdatedBy?.SystemProperties.Id;

        // Simplified approach - extract basic info using JSON serialization
        try
        {
            var assetJson = JsonConvert.SerializeObject(asset);
            var assetObj = Newtonsoft.Json.Linq.JObject.Parse(assetJson);

            if (assetObj["fields"] is Newtonsoft.Json.Linq.JObject fields)
            {
                // Extract title
                if (fields["title"] is Newtonsoft.Json.Linq.JObject titleField)
                {
                    var titleProp = titleField.Property("en-US") ?? titleField.Properties().FirstOrDefault();
                    Title = titleProp?.Value?.ToString();
                }

                // Extract file information
                if (fields["file"] is Newtonsoft.Json.Linq.JObject fileField)
                {
                    var fileProp = fileField.Property("en-US") ?? fileField.Properties().FirstOrDefault();
                    var fileObj = fileProp?.Value as Newtonsoft.Json.Linq.JObject;
                    if (fileObj != null)
                    {
                        FileName = fileObj["fileName"]?.ToString();
                        ContentType = fileObj["contentType"]?.ToString();

                        var details = fileObj["details"] as Newtonsoft.Json.Linq.JObject;
                        if (details != null)
                        {
                            var size = details["size"]?.ToString();
                            if (!string.IsNullOrEmpty(size) && long.TryParse(size, out var parsedSize))
                            {
                                FileSize = parsedSize;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // If JSON parsing fails, we'll have empty string properties
        }
    }

    public AssetEntity(ManagementAsset asset)
    {
        ContentId = asset.SystemProperties.Id;
        TagIds = asset.Metadata?.Tags.Select(x => x.Sys.Id) ?? Enumerable.Empty<string>();
        CreatedAt = asset.SystemProperties.CreatedAt;
        UpdatedAt = asset.SystemProperties.UpdatedAt;
        Version = asset.SystemProperties.Version ?? default;
        UpdatedBy = asset.SystemProperties.UpdatedBy?.SystemProperties.Id;

        // Extract title from Title property
        if (asset.Title != null && asset.Title.TryGetValue("en-US", out var titleValue))
        {
            Title = titleValue;
        }

        // Extract description from Description property
        if (asset.Description != null && asset.Description.TryGetValue("en-US", out var descValue))
        {
            Description = descValue;
        }

        // Extract file information from Files property
        if (asset.Files != null && asset.Files.TryGetValue("en-US", out var fileValue))
        {
            // Convert File to JSON to extract details
            var fileJson = JsonConvert.SerializeObject(fileValue);
            var fileObj = Newtonsoft.Json.Linq.JObject.Parse(fileJson);

            FileName = fileObj["fileName"]?.ToString();
            ContentType = fileObj["contentType"]?.ToString();

            var details = fileObj["details"] as Newtonsoft.Json.Linq.JObject;
            if (details != null)
            {
                var size = details["size"]?.ToString();
                if (!string.IsNullOrEmpty(size) && long.TryParse(size, out var parsedSize))
                {
                    FileSize = parsedSize;
                }
            }
        }
    }
}