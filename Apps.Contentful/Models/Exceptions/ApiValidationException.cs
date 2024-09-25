namespace Apps.Contentful.Models.Exceptions;

public class ApiValidationException(string message, List<string> validationErrors) : Exception(message)
{
    public List<string> ValidationErrors { get; } = validationErrors;

    public override string ToString()
    {
        var errors = string.Join("; ", ValidationErrors);
        return $"{Message} - {errors}";
    }
}
