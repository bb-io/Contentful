using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
