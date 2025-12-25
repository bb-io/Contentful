using Apps.Contentful.Api;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;
using Newtonsoft.Json.Linq;
using File = Contentful.Core.Models.File;

namespace Apps.Contentful.Utils;

class EntryCloner(ContentfulClient apiClient)
{
    private readonly Dictionary<string, ContentType> _contentTypesCached = [];

    public async Task<DuplicateEntryResponse> CloneRecursively(DuplicateEntryRequest request)
    {
        var rootEntryId = request.EntryId;
        var graph = new EntryConnectionsGraph([], []);
        var whitelistedFields = request.DuplicateFromFieldIds?.ToHashSet() ?? [];
        var recursionDepthReached = 0;

        // 1 Fetch root entry and add to the graph
        graph.Resources[rootEntryId] = new Content { Resource = await apiClient.GetEntry(rootEntryId) };
        graph.Connections.UnionWith(await ExtractConnections(graph.Resources[rootEntryId].Resource, whitelistedFields));

        if (!string.IsNullOrEmpty(request.NewRootContenTypeId))
        {
            graph.Resources[rootEntryId].Resource.SystemProperties.ContentType.SystemProperties.Id = request.NewRootContenTypeId;
        }


        // 2 For each connection where "To" is not in the graph.Entries
        //     fetch entries, add each to the graph and to the connections
        //     repeat until there are no connections without matching "To"

        // Fetch entries
        while (true)
        {
            var entryIdsToFetch = graph.Connections
                .Where(conn => conn.ToType == SystemLinkTypes.Entry && !graph.Resources.ContainsKey(conn.ToId))
                .Select(conn => conn.ToId);

            if (!entryIdsToFetch.Any())
                break;

            var entryFilter = new QueryBuilder<Entry<object>>()
                .FieldIncludes("sys.id", entryIdsToFetch);

            var fetchedEntries = await apiClient.GetEntriesCollection(entryFilter);

            foreach (var entry in fetchedEntries)
            {
                graph.Resources[entry.SystemProperties.Id] = new Content { Resource = entry };
                graph.Connections.UnionWith(await ExtractConnections(entry, whitelistedFields));
            }

            recursionDepthReached++;
        }

        // Fetch assets
        while (true)
        {
            var assetIdsToFetch = graph.Connections
                .Where(conn => conn.ToType == SystemLinkTypes.Asset && !graph.Resources.ContainsKey(conn.ToId))
                .Select(conn => conn.ToId);

            if (!assetIdsToFetch.Any())
                break;

            var assetFilter = new QueryBuilder<ManagementAsset>()
                .FieldIncludes("sys.id", assetIdsToFetch);

            var fetchedAssets = await apiClient.GetAssetsCollection(assetFilter);

            foreach (var asset in fetchedAssets)
            {
                graph.Resources[asset.SystemProperties.Id] = new Content { Resource = asset };
                // We assume that assets themselves cannot link to other resources
            }
        }

        // 3 For each entry that's in "To" without "From"
        //     create a clone with all data
        //     add to CloneMap: Original ID -> Clone ID

        var resourcesWithoutReferences = graph.Resources
            .Where(r => !graph.Connections.Any(c => c.FromId == r.Key));

        foreach (var pair in resourcesWithoutReferences)
        {
            switch (pair.Value.Resource)
            {
                case Entry<dynamic> entry:
                    await CloneEntry(pair.Value, entry);
                    break;

                case ManagementAsset asset:
                    if (request.EnableAssetCloning == false)
                    {
                        pair.Value.CloneId = asset.SystemProperties.Id;
                        break;
                    }

                    // We can't just copy asset references, we need to reupload files to be able to publish those assets later
                    var reuploadedFiles = new Dictionary<string, File>();
                    foreach (var file in asset.Files)
                    {
                        var currentFileUrl = file.Value.Url.StartsWith("//")
                            ? "https:" + file.Value.Url
                            : file.Value.Url;
                        using var httpClient = new HttpClient();
                        var fileContent = await httpClient.GetByteArrayAsync(new Uri(currentFileUrl));
                        var uploadReference = await apiClient.UploadFile(fileContent);
                        reuploadedFiles.Add(file.Key, new()
                        {
                            FileName = file.Value.FileName,
                            ContentType = file.Value.ContentType,
                            UploadReference = new UploadReference()
                            {
                                // passinng the upload reference object doesn't work,
                                // as it has wront type/link type values
                                SystemProperties = new SystemProperties()
                                {
                                    Id = uploadReference.SystemProperties.Id,
                                    Type = "Link",
                                    LinkType = "Upload",
                                }
                            },
                        });
                    }

                    var clonedAsset = await apiClient.CreateAsset(new()
                    {
                        Title = asset.Title,
                        Description = asset.Description,
                        Files = reuploadedFiles,
                    });

                    foreach (var locale in reuploadedFiles.Keys)
                    {
                        await apiClient.ProcessAsset(clonedAsset.SystemProperties.Id, clonedAsset.SystemProperties.Version ?? 1, locale);
                    }

                    pair.Value.CloneId = clonedAsset.SystemProperties.Id;
                    break;
            }
        }

        // 4 While there is an entry that's in "From" where all their "To" are in CloneMap
        //     create a clone with original data
        //     replace original data with CloneMap where needed
        //     add to CloneMap: Original ID -> Clone ID
        while (true)
        {
            var entryContent = FilterContentWithAllReferencesCloned(graph);

            if (entryContent.Count == 0)
                break;

            // missing connections can't be here becase we're iterating over content with clones for all outgoing connections
            foreach (var entryWithAllReferencesCloned in entryContent)
                await CloneWithReplacedConnections(entryWithAllReferencesCloned, graph.Resources, whitelistedFields);
        }


        // 5 While there are entries which are not in CloneMap -- this is a circular depedency here
        //     pick first of the remaining entries
        //     clone with original data
        //     remove missing links and add to LinksToUpdateMap
        //     run 4
        //     go through LinksToUpdateMap and paste missing links from CloneMap
        var pendingLinkBackfills = new HashSet<PendingLinkBackfill>();

        while (true)
        {
            var entry = graph.Connections
                .GroupBy(conn => conn.FromId)
                .Where(g =>
                    graph.Resources.ContainsKey(g.Key)                         // ensure 'From' entry was fetched, should be always true
                    && string.IsNullOrEmpty(graph.Resources[g.Key].CloneId))   // skip entries that already have clones
                .Select(g => graph.Resources[g.Key])
                .LastOrDefault();

            if (entry is null)
                break;

            pendingLinkBackfills.UnionWith(await CloneWithReplacedConnections(entry, graph.Resources, whitelistedFields));

            while (true)
            {
                var entryContent = FilterContentWithAllReferencesCloned(graph);

                if (entryContent.Count == 0)
                    break;

                // missing connections can't be here becase we're iterating over content with clones for all outgoing connections
                foreach (var entryWithAllReferencesCloned in entryContent)
                    await CloneWithReplacedConnections(entryWithAllReferencesCloned, graph.Resources, whitelistedFields);
            }
        }

        foreach (var backfillsForEntry in pendingLinkBackfills.GroupBy(b => b.ClonedEntryId))
        {
            var entryToBackfill = await apiClient.GetEntry(backfillsForEntry.Key);
            var entryFields = entryToBackfill.Fields as JObject;

            foreach (var backfill in backfillsForEntry)
            {
                if (backfill is null)
                    throw new InvalidOperationException("Link for backfilling a circular depency is null.");

                if (!graph.Resources.TryGetValue(backfill.OriginalToId, out var linkedContent))
                    throw new InvalidOperationException("Can't find original resource for backfilling a circular depency link.");

                if (string.IsNullOrEmpty(linkedContent.CloneId))
                    throw new InvalidOperationException("Couldn't clone resource for backfilling a circular depency link.");

                var backfillField = backfill.IsArrayElement
                    ? $"['{backfill.FieldId}']['{backfill.Locale}']"
                    : $"['{backfill.FieldId}']['{backfill.Locale}']";

                if (entryFields?.SelectToken(backfillField) is not JToken tokenToBackfill)
                    throw new InvalidOperationException("Couldn't locate a token for backfilling a circular depency link.");

                if (backfill.IsArrayElement && backfill.ArrayIndex is null)
                    throw new InvalidOperationException("Array index is not present for backfilling a cloned link.");

                if (backfill.IsArrayElement)
                    SetArrayToken(tokenToBackfill, linkedContent.CloneId, backfill.ArrayIndex!.Value);
                else
                    SetSingleToken(tokenToBackfill, linkedContent.CloneId);
            }

            await apiClient.CreateOrUpdateEntry(entryToBackfill, version: entryToBackfill.SystemProperties.Version);
        }

        var clonedResources = graph.Resources
            .Where(r => !string.IsNullOrEmpty(r.Value.CloneId) && r.Key != r.Value.CloneId)
            .ToList();

        return new()
        {
            RootEntry = new EntryEntity(await apiClient.GetEntry(graph.Resources[rootEntryId].CloneId)),

            RecursivelyClonedEntryIds = clonedResources
                .Where(r => r.Value.Resource is Entry<dynamic>
                    && r.Value.CloneId != graph.Resources[rootEntryId].CloneId)
                .Select(r => r.Value.CloneId ?? string.Empty)
                ?? [],

            RecursivelyClonedAssetIds = clonedResources
                .Where(r => r.Value.Resource is ManagementAsset)
                .Select(r => r.Value.CloneId ?? string.Empty)
                ?? [],

            TotalItemsCloned = clonedResources.GroupBy(r => r.Value.CloneId).Count(),
            RecursionDepthReached = recursionDepthReached,
        };
    }

    private static List<Content> FilterContentWithAllReferencesCloned(EntryConnectionsGraph graph)
    {
        return graph.Connections
                .GroupBy(conn => conn.FromId)
                .Where(g =>
                    graph.Resources.ContainsKey(g.Key)                         // ensure 'From' entry was fetched, should be always true
                    && string.IsNullOrEmpty(graph.Resources[g.Key].CloneId)    // skip entries that already have clones
                    && g.All(conn =>                                           // pick 'From' where all all their 'To' were already cloned
                        graph.Resources.ContainsKey(conn.ToId)
                        && !string.IsNullOrEmpty(graph.Resources[conn.ToId].CloneId)))
                .Select(g => graph.Resources[g.Key])
                .ToList();
    }

    private static JObject CreateLinkObject(string cloneId)
    {
        return new JObject
        {
            ["sys"] = new JObject
            {
                ["type"] = "Link",
                ["linkType"] = "Entry",
                ["id"] = cloneId
            }
        };
    }

    private static void SetSingleToken(JToken token, string cloneId)
    {
        if (token.SelectToken("sys.id") is JValue idToken)
            idToken.Value = cloneId;
        else
            token.Replace(CreateLinkObject(cloneId));
    }

    private static void SetArrayToken(JToken token, string cloneId, int insertIndex)
    {
        if (token is not JArray targetArray)
        {
            targetArray = [];
            token.Replace(targetArray);
        }

        while (targetArray.Count <= insertIndex)
            targetArray.Add(JValue.CreateNull());

        JToken tokenAtIndex = targetArray[insertIndex];
        SetSingleToken(tokenAtIndex, cloneId);
    }

    private async Task<IEnumerable<PendingLinkBackfill>> CloneWithReplacedConnections(
        Content content,
        Dictionary<string, Content> fetchedContent,
        HashSet<string> whitelistedFields)
    {
        var draftBackfills = new List<PendingLinkBackfill>();

        if (content.Resource is not Entry<dynamic> entry)
            return draftBackfills;

        var contentType = await GetContentTypeWithCache(entry);
        var clone = new Entry<dynamic>
        {
            SystemProperties = new() { ContentType = contentType },
            Fields = entry.Fields is JObject fields
                ? fields.DeepClone()
                : new JObject(),
        };
        var cloneFields = clone.Fields as JObject ?? [];

        foreach (var field in contentType.Fields)
        {
            if (!whitelistedFields.Contains(field.Id))
                continue;

            if (!cloneFields.TryGetValue(field.Id, out var fieldValue))
                continue;

            if (fieldValue is null)
                continue;

            switch (field.Type)
            {
                case SystemFieldTypes.Link:
                    foreach (var locale in fieldValue.Children<JProperty>())
                    {
                        if (ReplaceReferenceId(locale.Value, fetchedContent) is string missingCloneId)
                        {
                            var backfill = new PendingLinkBackfill(string.Empty, missingCloneId, field.Id, locale.Name, IsArrayElement: false);
                            draftBackfills.Add(backfill);
                        }
                    }

                    break;

                case SystemFieldTypes.Array:
                    foreach (var locale in fieldValue.Children<JProperty>())
                    {
                        if (locale.Value is not JArray array)
                            break;

                        foreach (var (link, index) in array.Select((item, index) => (item, index)))
                        {
                            if (ReplaceReferenceId(link, fetchedContent) is string missingCloneId)
                            {
                                var backfill = new PendingLinkBackfill(string.Empty, missingCloneId, field.Id, locale.Name, IsArrayElement: true, ArrayIndex: index);
                                draftBackfills.Add(backfill);
                            }
                        }
                    }
                    break;

                    // TODO Document that references from RichText fields are not supported

                    // TODO Document that links to another spaces are not supported
            }
        }

        await CloneEntry(content, clone);
        return content.CloneId is not null
            ? draftBackfills.Select(b => b with { ClonedEntryId = content.CloneId })
            : throw new Exception("Entry clone failed");
    }

    private async Task<HashSet<ContentConnection>> ExtractConnections(
        IContentfulResource resource,
        HashSet<string> whitelistedFields)
    {
        var connections = new HashSet<ContentConnection>();

        if (resource is Asset)
            return connections;

        if (resource is not Entry<dynamic> entry)
            throw new Exception("An unexpected resource received for extraction its connections.");

        var contentType = await GetContentTypeWithCache(entry);
        var fromEntryId = entry.SystemProperties.Id;
        var entryFields = entry.Fields as JObject ?? [];

        foreach (var field in contentType.Fields)
        {
            if (!whitelistedFields.Contains(field.Id))
                continue;

            if (!entryFields.TryGetValue(field.Id, out var fieldValue))
                continue;

            if (fieldValue is null)
                continue;

            switch (field.Type)
            {
                case SystemFieldTypes.Link:
                    foreach (var locale in fieldValue.Children<JProperty>())
                        AddConnection(connections, fromEntryId, locale.Value, field.LinkType);
                    break;

                case SystemFieldTypes.Array:
                    foreach (var locale in fieldValue.Children<JProperty>())
                    {
                        if (locale.Value is not JArray array)
                            break;

                        foreach (var link in array)
                            AddConnection(connections, fromEntryId, link, link["sys"]?["linkType"]?.ToString() ?? string.Empty);
                    }
                    break;

                    // TODO Document that references from RichText fields are not supported

                    // TODO Document that links to another spaces are not supported
            }
        }

        return connections;
    }

    public async Task<ContentType> GetContentTypeWithCache(Entry<dynamic> entry)
    {
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;

        if (_contentTypesCached.TryGetValue(contentTypeId, out var cachedContentType))
            return cachedContentType;

        var fetchedContentType = await apiClient.GetContentType(contentTypeId);
        _contentTypesCached.Add(contentTypeId, fetchedContentType);
        return fetchedContentType;
    }

    private static void AddConnection(
        HashSet<ContentConnection> connections,
        string fromEntryId,
        JToken? token,
        string linkToType)
    {
        var referencedToId = token?["sys"]?["id"]?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(referencedToId))
            return;

        var connection = new ContentConnection(
            fromEntryId,
            referencedToId,
            linkToType
        );

        connections.Add(connection);
    }

    private static string? ReplaceReferenceId(JToken? token, Dictionary<string, Content> resources)
    {
        if (token?["sys"] is not JObject sys)
            throw new ArgumentException("Unexpected token received for replacing reference IDs.");

        var originalToId = (sys["id"]?.ToString())
            ?? throw new ArgumentException("No original ID was received for replacing reference IDs.");

        var cloneId = resources
            .Where(r => r.Key == originalToId && !string.IsNullOrEmpty(r.Value.CloneId))
            .Select(r => r.Value.CloneId)
            .FirstOrDefault();

        if (cloneId is null)
        {
            sys["id"] = null;
            return originalToId;
        }

        sys["id"] = cloneId;
        return null;
    }

    private async Task CloneEntry(Content content, Entry<dynamic> entry)
    {
        var cloneContentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var clone = new Entry<dynamic>
        {
            Fields = entry.Fields is JObject fields
                ? fields.DeepClone()
                : new JObject()
        };

        var clonedEntry = await apiClient.CreateEntry(clone, cloneContentTypeId);
        content.CloneId = clonedEntry.SystemProperties.Id;
    }
}

record EntryConnectionsGraph(
    Dictionary<string, Content> Resources,
    HashSet<ContentConnection> Connections);

class Content()
{
    public required IContentfulResource Resource { get; init; }
    public string? CloneId { get; set; } = null;
}

record ContentConnection(
    string FromId,
    string ToId,
    string ToType);

record PendingLinkBackfill(
    string ClonedEntryId,
    string OriginalToId,
    string FieldId,
    string Locale,
    bool IsArrayElement,
    int? ArrayIndex = null);