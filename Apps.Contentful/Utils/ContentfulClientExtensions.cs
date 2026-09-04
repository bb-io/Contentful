using Blackbird.Applications.Sdk.Common.Exceptions;
using Contentful.Core.Models;

namespace Apps.Contentful.Utils;

public static class ContentfulClientExtensions
{
    public static async Task<Entry<dynamic>?> GetEntryWithErrorHandling(this Api.ContentfulClient contentfulClient, string entryId)
    {
        try
        {
            return await contentfulClient.ExecuteWithErrorHandling(() => contentfulClient.GetEntry(entryId));
        }
        catch (PluginApplicationException e)
        {
            throw new PluginApplicationException(
                $"Couldn't get an entry with ID: {entryId}. Contentful response: {e.Message}",
                e);
        }
    }

    public static void ThrowIfNullOrEmpty(this string? value, string paramerName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new PluginApplicationException($"Parameter '{paramerName}' is null or empty. Please provide a valid value.");
        }
    }
}