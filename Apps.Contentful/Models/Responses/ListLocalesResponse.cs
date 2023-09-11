using Apps.Contentful.Models.Identifiers;

namespace Apps.Contentful.Models.Responses;

public class ListLocalesResponse
{
    public IEnumerable<LocaleIdentifier> Locales { get; set; }
}