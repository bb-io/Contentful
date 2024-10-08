﻿using Apps.Contentful.Webhooks.Models.Payload;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class AssetChangedResponse
{
    [Display("Asset ID")]
    public string AssetId { get; set; }

    [Display("Files info")]
    public List<AssetFileInfo> FilesInfo { get; set; }
}