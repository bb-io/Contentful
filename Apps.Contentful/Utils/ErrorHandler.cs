using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Utils;

public static class ErrorHandler
{
    private static readonly Dictionary<string, string> ErrorMappings = new()
    {
        { KnownErrorMessages.CouldNotDetectContentType, "This file type is not supported" },
        { KnownErrorMessages.CannotConvertToContent, "Cannot generate the original document format. The uploaded XLIFF is missing the original file's structural data." }
    };

    public static T ExecuteWithErrorHandling<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            var mapping = ErrorMappings.FirstOrDefault(m => ex.Message.Contains(m.Key));
            if (mapping.Key != null)
                throw new PluginMisconfigurationException(mapping.Value);

            throw new PluginApplicationException(ex.Message);
        }
    }
}