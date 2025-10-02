using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Models;

public record EntryContentDto(string Id, JObject EntryFields, Field[] ContentTypeFields, User UpdatedBy);