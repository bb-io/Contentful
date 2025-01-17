using Blackbird.Applications.Sdk.Common.Exceptions;
using Contentful.Core.Errors;

namespace Apps.Contentful.Utils;

public static class ExceptionWrapper
{
    public static async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> func)
    {
        try
        {
            return await func.Invoke();
        }
        catch (ContentfulException e)
        {
            throw new PluginApplicationException(e.Message);
        }
    }
    
    public static async Task ExecuteWithErrorHandling(Func<Task> func)
    {
        try
        {
            await func.Invoke();
        }
        catch (ContentfulException e)
        {
            throw new PluginApplicationException(e.Message);
        }
    }
}