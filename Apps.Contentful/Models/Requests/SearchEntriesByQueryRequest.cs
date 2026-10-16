using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class SearchEntriesByQueryRequest : ContentModelOptionalIdentifier
{
    [Display("Search term", Description = "Search entry content, including JSON values. Map a flight UUID here to find candidate entries. The value is URL-encoded automatically.")]
    public string? SearchTerm { get; set; }

    [Display("Order", Description = "Contentful sort order, for example -sys.updatedAt,sys.id. Defaults to sys.id. Determines which matching entry is returned first.")]
    public string? Order { get; set; }
}
