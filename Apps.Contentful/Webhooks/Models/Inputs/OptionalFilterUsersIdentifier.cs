using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Webhooks.Models.Inputs;
public class OptionalFilterUsersIdentifier
{
    [Display("User IDs to filter", Description = "The event won't be triggered by any of these users"), DataSource(typeof(UserDataSourceHandler))]
    public IEnumerable<string>? UserIds { get; set; }
}
