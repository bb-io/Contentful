using Apps.Contentful.Api;
using Contentful.Core.Models;

namespace Apps.Contentful.Utils.Cache;

public class ContentTypeCache(ContentfulClient client)
{
    private readonly Dictionary<string, ContentType> _contentTypes = new();

    public async Task<ContentType> Get(string contentTypeId)
    {
        if (_contentTypes.TryGetValue(contentTypeId, out var cachedContentType))
            return cachedContentType;
        
        var contentType = await client.GetContentType(contentTypeId);
        _contentTypes[contentTypeId] = contentType;
        return contentType;
    }
}