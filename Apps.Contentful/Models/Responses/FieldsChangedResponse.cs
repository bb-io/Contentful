using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Responses
{
    public class FieldsChangedResponse
    {
        public string EntryId { get; set; }
        public List<FieldObject> Fields { get; set; }
    }

    public class FieldObject
    {
        public string FieldId { get; set; }

        public string Locale { get; set; }

        public string FieldValue { get; set; }
    }
}
