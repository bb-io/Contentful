using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Identifiers;
public class OptionalEntryIdentifier : EnvironmentIdentifier
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string? EntryId { get; set; }
}
