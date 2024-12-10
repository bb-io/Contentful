using Blackbird.Applications.Sdk.Common.Exceptions;
using Contentful.Core.Errors;
using Contentful.Core.Models;

namespace Apps.Contentful.Utils;

public static class ContentfulClientExtensions
{
    public static async Task<Entry<dynamic>?> GetEntryWithErrorHandling(this Api.ContentfulClient contentfulClient, string entryId)
    {
        try
        {
            return await contentfulClient.GetEntry(entryId);
        }
        catch (ContentfulException e)
        {
            throw new PluginApplicationException($"Couldn't get an entry with ID: {entryId}. Contentful response: {e.Message}");
        }
    }
}