using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Filters.Coders;
using Newtonsoft.Json;
using System.Text;
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
            ContentId = "70rcIvfhhD9VjLlMTA3YPz",
            Locale = "en-US"
        };
        var request = new GetEntryAsHtmlRequest();
        
        var fileResponse = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, request);
        
        IsFalse(string.IsNullOrEmpty(fileResponse.Content.ToString()));
    }
    
    //[TestMethod]
    //public async Task SetEntryLocalizableFieldsFromHtmlFile_WithoutReferenceEntries_ShouldNotFail()
    //{
    //    var entryActions = new EntryActions(InvocationContext, FileManager);
    //    var entryIdentifier = new UploadEntryRequest()
    //    {
    //        Environment = "dev",
    //        Locale = "nl",
    //        Content = new()
    //        {
    //            Name = "Mr. Coffee Mug Warmer_en-US.html",
    //            ContentType = "text/html"
    //        }
    //    };
        
    //    await entryActions.SetEntryLocalizableFieldsFromHtmlFile(entryIdentifier);
    //}

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

    //[TestMethod]
    //public async Task GetEntry_ValidEntryWithLocale_ShouldReturnEntryWithTitle()
    //{
    //    var entryActions = new EntryActions(InvocationContext, FileManager);
    //    var entryIdentifier = new EntryIdentifier
    //    {
    //        Environment = "master",
    //        EntryId = "5N76zvCw2PMHTE2rY6pnCo"
    //    };
    //    var localeIdentifier = new LocaleOptionalIdentifier
    //    {
    //        Locale = "en-US"
    //    };

    //    var entry = await entryActions.GetEntry(entryIdentifier, localeIdentifier);
    //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(entry, Formatting.Indented);
    //    Console.WriteLine(json);

    //    IsNotNull(entry);
    //    IsNotNull(entry.Title);
    //    IsNotNull(entry.TagIds);
        
    //    Console.WriteLine($"Entry title: {entry.Title}");
    //}

    
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


    //[TestMethod]
    //public async Task SetEntryTextField_WithReferenceEntry_ShouldNotFail()
    //{
    //    var entryActions = new EntryFieldActions(InvocationContext);
    //    var entryIdentifier = new EntryLocaleIdentifier()
    //    {
    //        Locale= "en-US",
    //        EntryId= "5FBuCuJwXpF5UhW3N9oYve",
    //        Environment = "master"
    //    };

    //    var fieldIdentifier = new FieldIdentifier()
    //    {
    //        FieldId= "text_sanitized",
            
    //    };

    //    await entryActions.SetTextFieldContent(entryIdentifier, fieldIdentifier, "a {Open ‹a href=\"{url}\" data-q=\"x & y\"›{label}‹/a› — {when, time, short}}");
    //}


    [TestMethod]
    public async Task DownloadEntry_ShouldNotFail()
    {
        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = "en-US",
            ContentId= "5746dLKTkEZjOQX21HX2KI",
            
        };
        var entry = new GetEntryAsHtmlRequest
        {
            IgnoredFieldIds = new List<string>
           {
                "slug"
           },
            GetHyperlinkContent = true,
            IgnoredContentTypeIds = new List<string>
            {
                "page"
            },
            GetEmbeddedBlockContent = true,
            GetEmbeddedInlineContent = true,
            GetNonLocalizationReferenceContent = true
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task DownloadEntry_Has_Blacklake_required_fields()
    {
        var lang = "en-US";
        var contentId = "5746dLKTkEZjOQX21HX2KI";

        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = lang,
            ContentId = contentId,

        };
        var entry = new GetEntryAsHtmlRequest
        {
            GetReferenceContent = true,
            GetEmbeddedInlineContent = true,
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        var contentString = FileManager.ReadOutputAsString(response.Content);
        var codedContent = (new HtmlContentCoder()).Deserialize(contentString, response.Content.Name);

        foreach (var unit in codedContent.TextUnits.Where(x => x.Key is null))
        {
            Console.WriteLine(unit.GetCodedText());
        }

        Console.WriteLine(contentString);
        Assert.AreEqual(lang, codedContent.Language);
        Assert.AreEqual(contentId, codedContent.SystemReference.ContentId);
        Assert.AreEqual($"https://app.contentful.com/spaces/12ktqqmw656e/entries/{contentId}", codedContent.SystemReference.AdminUrl);
        Assert.AreEqual("Contentful", codedContent.SystemReference.SystemName);
        Assert.AreEqual("https://www.contentful.com", codedContent.SystemReference.SystemRef);
        Assert.IsNotNull(codedContent.SystemReference.ContentName);

        Assert.IsNotNull(codedContent.Provenance.Review.Person);
        Console.WriteLine(codedContent.Provenance.Review.Person);
        Assert.AreEqual("Contentful", codedContent.Provenance.Review.Tool);
        Assert.AreEqual("https://www.contentful.com", codedContent.Provenance.Review.ToolReference);

        Assert.IsTrue(codedContent.TextUnits.All(x => x.Key is not null));
    }

    [TestMethod]
    public async Task DownloadEntry_Has_Size_Restrictions()
    {
        var lang = "en-US";
        var contentId = "7pB35K9cZjsZrBmnUjwjLa";

        var entryActions = new EntryActions(InvocationContext, FileManager);
        var entryIdentifier = new DownloadContentInput
        {
            Locale = lang,
            ContentId = contentId,

        };
        var entry = new GetEntryAsHtmlRequest
        {
            GetReferenceContent = true,
            GetEmbeddedInlineContent = true,
        };

        var response = await entryActions.GetEntryLocalizableFieldsAsHtmlFile(entryIdentifier, entry);

        Console.WriteLine(response.Content.Name);
        var contentString = FileManager.ReadOutputAsString(response.Content);
        var codedContent = (new HtmlContentCoder()).Deserialize(contentString, response.Content.Name);

        Console.WriteLine(contentString);

        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 256));
        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 60));
        Assert.IsTrue(codedContent.TextUnits.Any(x => x.SizeRestrictions.MaximumSize == 160));
    }
}