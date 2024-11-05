﻿using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Contentful.Models.Requests;

public class GetIdsFromFileRequest : LocaleIdentifier
{
    public FileReference File { get; set; }

    [Display("Ignore localization field")]
    public bool? IgnoreLocalizationField { get; set; }
}
