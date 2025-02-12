using Apps.Contentful.Actions;
using Apps.Contentful.Models.Requests;
using ContentfulTests.Base;

namespace Tests.Contentful
{
    [TestClass]
    public class DataSources : TestBase
    {
        [TestMethod]
        public async Task SearchEntriesReturnsValues()
        {
            var action = new EntryActions(InvocationContext,FileManager);
            var input = new ListEntriesRequest {Environment= "temp_empty_for_test" };
            var result = await action.ListEntries(input);

            foreach (var entry in result.Entries)
            {
                Console.WriteLine(entry.Id);
            }
        }

    }
}
