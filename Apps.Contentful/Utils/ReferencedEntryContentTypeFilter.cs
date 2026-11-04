namespace Apps.Contentful.Utils;

public class ReferencedEntryContentTypeFilter(
    IEnumerable<string>? excludedContentTypeIds,
    IEnumerable<string>? includedContentTypeIds)
{
    private readonly HashSet<string> _excludedContentTypeIds = new(excludedContentTypeIds ?? [], StringComparer.Ordinal);
    private readonly HashSet<string> _includedContentTypeIds = new(includedContentTypeIds ?? [], StringComparer.Ordinal);

    public bool Allows(string contentTypeId)
    {
        if (_excludedContentTypeIds.Contains(contentTypeId))
            return false;

        return _includedContentTypeIds.Count == 0 || _includedContentTypeIds.Contains(contentTypeId);
    }
}
