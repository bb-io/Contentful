using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful
{
    public class ContentfulApplication : IApplication
    {
        private string _name;

        public ContentfulApplication()
        {
            _name = "Contentful";
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public T GetInstance<T>()
        {
            throw new NotImplementedException();
        }
    }
}
