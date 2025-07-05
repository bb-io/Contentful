using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Requests.Tags;
public class UploadEntryRequest : EnvironmentIdentifier, IUploadContentInput
{
    [Display("Entry ID")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string? ContentId { get; set; }

    [DataSource(typeof(LocaleDataSourceHandler))]
    public string Locale { get; set; }

    public FileReference Content { get; set; }

    [Display("Don't update reference fields")]
    public bool? DontUpdateReferenceFields { get; set; }
}
