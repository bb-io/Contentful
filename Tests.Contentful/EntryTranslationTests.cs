using Apps.Contentful.Actions;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Filters.Coders;
using Blackbird.Filters.Transformations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class EntryTranslationTests : TestBase
{
    [TestMethod]
    public async Task Translate_standard_html_file()
    {
        var actions = new EntryActions(InvocationContext, FileManager);
        await actions.SetEntryLocalizableFieldsFromHtmlFile(new Apps.Contentful.Models.Requests.Tags.UploadEntryRequest { Content = new FileReference { Name = "contentful-pseudo.html" }, Locale = "de" });

    }

    [TestMethod]
    public async Task Translate_from_xliff_file()
    {
        var actions = new EntryActions(InvocationContext, FileManager);
        var response = await actions.SetEntryLocalizableFieldsFromHtmlFile(new Apps.Contentful.Models.Requests.Tags.UploadEntryRequest { Content = new FileReference { Name = "contentful.html.xlf" }, Locale = "nl" });

        var contentString = FileManager.ReadOutputAsString(response.Content);
        var transformation = Transformation.Parse(contentString, response.Content.Name);

        Assert.AreEqual("5746dLKTkEZjOQX21HX2KI", transformation.TargetSystemReference.ContentId);
        Assert.AreEqual("nl", transformation.TargetLanguage);

        Console.WriteLine(JsonConvert.SerializeObject(transformation.TargetSystemReference, Formatting.Indented));
    }

    [TestMethod]
    public async Task Translate_from_deepl_updates_proper_fields()
    {
        var actions = new EntryActions(InvocationContext, FileManager);
        var response = await actions.SetEntryLocalizableFieldsFromHtmlFile(new Apps.Contentful.Models.Requests.Tags.UploadEntryRequest { Content = new FileReference { Name = "The Loire Valley_en-US.html.xlf" }, Locale = "nl" });

        var contentString = FileManager.ReadOutputAsString(response.Content);
        var transformation = Transformation.Parse(contentString, response.Content.Name);

        Assert.AreEqual("5746dLKTkEZjOQX21HX2KI", transformation.TargetSystemReference.ContentId);
        Assert.AreEqual("nl", transformation.TargetLanguage);

        Console.WriteLine(JsonConvert.SerializeObject(transformation.TargetSystemReference, Formatting.Indented));
    }
}
