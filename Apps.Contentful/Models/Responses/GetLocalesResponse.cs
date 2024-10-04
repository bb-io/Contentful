using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Responses
{
    public class GetLocalesResponse
    {
        [Display("Default locale")]
        public string DefaultLocale { get; set; }

        [Display("Other locales")]
        public IEnumerable<string> OtherLocales { get; set;}
    }
}
