using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.Contentful.Base;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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

        await ThrowsExceptionAsync<PluginApplicationException>(() => listEntriesTask);
    }
    
    [TestMethod]
    public async Task ListEntries_ValidEnvironment_ShouldReturnAllEntries()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var listEntriesRequest = new ListEntriesRequest { Environment = "dev", SearchTerm= "The perfect partner" };
        
        var entriesResponse = await entryActions.ListEntries(listEntriesRequest);

        foreach (var entry in entriesResponse.Entries)
        {
            Console.WriteLine($"{entry.ContentId}");
        }
    }

    [TestMethod]
    public async Task GetEntryLocalizableFieldsAsHtmlFile_WithoutReferenceEntries_ShouldGenerateHtmlFile()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput()
        {
            Environment = "dev",
            ContentId = "1lcz5tFE8v5JqylInsPvPW",
            Locale = "en-US"
        };
        var request = new GetEntryAsHtmlRequest();
        
        var fileResponse = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, request);
        
        IsFalse(string.IsNullOrEmpty(fileResponse.Content.ToString()));
    }
    
    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithoutReferenceEntries_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "5wFvto8Zhatz451gTDEpvP_en-US.html",
                ContentType = "text/html"
            }
        };
        
        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithHyperlinkEntries_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "Example_en-US.html",
                ContentType = "text/html"
            }
        };
        
        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }
    
    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithReferenceEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "First reference entry_en-US.html",
                ContentType = "text/html"
            },
            DontUpdateReferenceFields = true
        };
        
        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task SetEntryLocalizableFieldsFromHtmlFile_WithMarkdownEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new UploadEntryRequest()
        {
            Environment = "dev",
            Locale = "nl",
            Content = new()
            {
                Name = "Markdown entry #1_en-US.html",
                ContentType = "text/html"
            },
            DontUpdateReferenceFields = true
        };
        
        await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    }

    [TestMethod]
    public async Task GetEntry_ValidEntryWithLocale_ShouldReturnEntryWithTitle()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new EntryIdentifier
        {
            Environment = "dev",
            EntryId = "3Y4pOwtBOf3g47dh55LcyO"
        };
        var localeIdentifier = new LocaleOptionalIdentifier
        {
            Locale = "en-US"
        };

        var entry = await entryActions.GetEntry(entryIdentifier, localeIdentifier);

        IsNotNull(entry);
        IsNotNull(entry.Title);
        IsNotNull(entry.TagIds);
        
        Console.WriteLine($"Entry title: {entry.Title}");
    }

    
    [TestMethod]
    public async Task GetEntry_ValidEntryWithoutLocale_ShouldReturnEntryWithTitle()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new EntryIdentifier
        {
            Environment = "dev",
            EntryId = "5wFvto8Zhatz451gTDEpvP"
        };
        var localeIdentifier = new LocaleOptionalIdentifier();

        var entry = await entryActions.GetEntry(entryIdentifier, localeIdentifier);

        IsNotNull(entry);
        IsNotNull(entry.Title);
        Console.WriteLine($"Entry title: {entry.Title}");
    }


    [TestMethod]
    public async Task AddTagEntry_ShouldReturnEntryWithTag()
    {
        var entryActions = new TagActions(InvocationContext);
        var entryIdentifier = new EntryTagIdentifier
        {
            TagId = "23232",
            EntryId = "2kgmrDpJKTSqk3prI14WjX"
        };

        await entryActions.AddEntryTag(entryIdentifier);


        Assert.IsTrue(true);
    }
}