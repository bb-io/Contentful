using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Contentful.Models.Wrappers
{
    public class ItemWrapper<T>
    {
        public int Total { get; set; }
        public int SKip { get; set; }
        public int Limit { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
