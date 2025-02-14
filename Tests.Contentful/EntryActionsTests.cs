using Apps.Contentful.Actions;
using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class EntryActionsTests : TestBase
{
    [TestMethod]
    public async Task ListEntries_NotExistingEnvironment_ShouldFailWithException()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest { Environment = "temp_empty_for_test" };
        var listEntriesTask = entryActions.ListEntries(listEntriesRequest);

        await Assert.ThrowsExceptionAsync<PluginApplicationException>(() => listEntriesTask);
    }
}