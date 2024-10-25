using Apps.Contentful.DataSourceHandlers.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers
{
    public class OptionalTagListIdentifier : EnvironmentIdentifier
    {
        [Display("Tag IDs",
            Description = "Filter the incoming events to only trigger when all of the following tags are present")]
        [DataSource(typeof(OptionalTagDataHandler))]
        public IEnumerable<string>? TagIds { get; set; }

        [Display("Exclude tag IDs",
            Description = "Filter the incoming events to only trigger when all of the following tags are not present")]
        [DataSource(typeof(OptionalTagDataHandler))]
        public IEnumerable<string>? ExcludeTags { get; set; }
    }
}