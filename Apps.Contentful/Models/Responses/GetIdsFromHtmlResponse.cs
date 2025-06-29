﻿using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class GetIdsFromHtmlResponse
{
    [Display("Entry ID")]
    public string EntryId { get; set; } = string.Empty;

    [Display("Field ID")]
    public string FieldId { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;

    [Display("Linked entry IDs")]
    public List<string> LinkedEntryIds { get; set; } = new();

    [Display("Linked asset IDs")]
    public List<string> LinkedAssetIds { get; set; } = new();
}