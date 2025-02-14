using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Models.Exceptions;

public class FieldConversionException(string fieldId, string inputValue, string expectedType)
    : Exception(
        $"Field '{fieldId}' expects a {expectedType} value, but received '{inputValue}'. Please correct the input.")
{
    public string FieldId { get; } = fieldId;
    public string InputValue { get; } = inputValue;
    public string ExpectedType { get; } = expectedType;
}
