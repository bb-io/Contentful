using Apps.Contentful.Models.Entities;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Webhooks.Models.Outputs;
public class EntryWithPublishedLocales : EntryEntity
{
    public EntryWithPublishedLocales(EntryEntity other) : base(other)
    {
    }

    [Display("Published locales", Description = "All locales that were selected when publishing this entry")]
    public IEnumerable<string> PublishedLocales { get; set; } = [];

}
