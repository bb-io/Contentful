namespace Apps.Contentful.Models.Wrappers
{
    public class ItemWrapper<T>
    {
        public int Total { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public IEnumerable<T>? Items { get; set; }
    }
}
