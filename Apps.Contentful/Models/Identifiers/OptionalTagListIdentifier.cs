using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers
{
    public class OptionalTagListIdentifier
    {
        [Display("Tag IDs", Description = "Filter the incoming events to only trigger when all of the following tags are present")]
        [DataSource(typeof(TagDataHandler))]
        public IEnumerable<string>? TagIds { get; set; }
    }
}
