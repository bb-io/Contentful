﻿using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses;

public class ListEntriesResponse(IEnumerable<EntryEntity> entries, double totalCount)
{
    public IEnumerable<EntryEntity> Entries { get; set; } = entries;

    [Display("Total count")]
    public double TotalCount { get; set; } = totalCount;
}