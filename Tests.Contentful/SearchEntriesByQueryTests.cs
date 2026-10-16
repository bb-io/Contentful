using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Apps.Contentful.Actions;
using Apps.Contentful.Api;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class SearchEntriesByQueryTests : TestBase
{
    public SearchEntriesByQueryTests()
    {
        // Test settings may contain a Bearer prefix; the SDK supplies its own.
        Credentials = Credentials.Select(credential => new AuthenticationCredentialsProvider(credential.KeyName,
            credential.KeyName == "Authorization" && credential.Value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? credential.Value[7..].Trim() : credential.Value)).ToArray();
        InvocationContext = new InvocationContext { AuthenticationCredentialsProviders = Credentials };
    }

    [TestMethod]
    [TestCategory("Offline")]
    public async Task SearchEntriesByQuery_OptionalInputs_AreEncodedAutomatically()
    {
        const string term = "a+b & c/d? #50% \"café\"";
        var (response, requests) = await RunOfflineAsync(new() { SearchTerm = " " + term + " ", ContentModelId = " article ", Order = " -sys.updatedAt,sys.id " }, []);
        AssertResponse(response);
        Assert.IsNull(response.FirstEntry);
        var parameters = HttpUtility.ParseQueryString(requests.Single().Query);
        Assert.AreEqual(term, parameters["query"]);
        Assert.AreEqual("article", parameters["content_type"]);
        Assert.AreEqual("-sys.updatedAt,sys.id", parameters["order"]);
        Assert.AreEqual("sys,metadata.tags", parameters["select"]);
        Assert.AreEqual(JTokenType.Null, JObject.FromObject(response)["FirstEntry"]!.Type);
    }

    [DataTestMethod]
    [TestCategory("Offline")]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    public async Task SearchEntriesByQuery_BlankInputs_ListEntriesWithDefaultOrder(string? value)
    {
        var (response, requests) = await RunOfflineAsync(new() { SearchTerm = value, ContentModelId = value, Order = value }, [Entry("native")]);
        AssertResponse(response, "native");
        var parameters = HttpUtility.ParseQueryString(requests.Single().Query);
        Assert.AreEqual("sys.id", parameters["order"]);
        Assert.IsNull(parameters["query"]);
        Assert.IsNull(parameters["content_type"]);
        Assert.AreEqual("sys,metadata.tags", parameters["select"]);
        Assert.AreEqual("0", parameters["skip"]);
        Assert.AreEqual("100", parameters["limit"]);
        Assert.IsTrue(requests.Single().AbsolutePath.Contains("/environments/selected/entries", StringComparison.Ordinal));
    }

    [DataTestMethod]
    [TestCategory("Offline")]
    [DataRow(0)]
    [DataRow(100)]
    [DataRow(101)]
    public async Task SearchEntriesByQuery_PaginatesAllEntriesAndPreservesFilters(int count)
    {
        var candidates = Enumerable.Range(0, count).Select(index => Entry($"id{index:D3}")).ToArray();
        var (response, requests) = await RunOfflineAsync(new() { SearchTerm = "one two", ContentModelId = "article", Order = "-sys.updatedAt,sys.id" }, candidates);
        AssertResponse(response, candidates.Select(entry => entry["sys"]!["id"]!.Value<string>()!).ToArray());
        var expectedSkips = count < 100 ? new[] { "0" } : new[] { "0", "100" };
        CollectionAssert.AreEqual(expectedSkips, requests.Select(uri => HttpUtility.ParseQueryString(uri.Query)["skip"]).ToArray());
        foreach (var uri in requests)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            Assert.AreEqual("one two", parameters["query"]);
            Assert.AreEqual("article", parameters["content_type"]);
            Assert.AreEqual("-sys.updatedAt,sys.id", parameters["order"]);
            Assert.AreEqual("100", parameters["limit"]);
            Assert.AreEqual("sys,metadata.tags", parameters["select"]);
        }
    }

    [TestMethod]
    [TestCategory("Offline")]
    public async Task SearchEntriesByQuery_Response_PreservesNativeOrderAndCompleteEntryMapping()
    {
        var published = Entry("z");
        published["sys"]!["publishedVersion"] = 3;
        var changed = Entry("a");
        changed["sys"]!["publishedVersion"] = 2;
        var draft = Entry("draft");
        draft["sys"]!["archivedVersion"] = 3;
        draft.Remove("metadata");
        var (response, requests) = await RunOfflineAsync(new() { SearchTerm = "native", Order = "-sys.updatedAt" }, [published, changed, draft]);
        AssertResponse(response, "z", "a", "draft");
        var entries = response.Entries.ToArray();
        Assert.IsTrue(entries[0].IsPublished);
        Assert.IsFalse(entries[0].IsDraft);
        Assert.IsFalse(entries[0].IsChanged);
        Assert.IsTrue(entries[1].IsChanged);
        Assert.IsFalse(entries[1].IsPublished);
        Assert.IsTrue(entries[2].IsDraft);
        Assert.IsTrue(entries[2].IsArchived);
        Assert.IsFalse(entries[0].IsArchived);
        Assert.AreEqual("model", entries[0].ContentTypeId);
        Assert.AreEqual("editor", entries[0].UpdatedBy);
        Assert.AreEqual(4, entries[0].Version);
        Assert.AreEqual(DateTime.Parse("2026-10-16T12:00:00Z").ToUniversalTime(), entries[0].CreatedAt!.Value.ToUniversalTime());
        Assert.AreEqual(DateTime.Parse("2026-10-16T13:00:00Z").ToUniversalTime(), entries[0].UpdatedAt!.Value.ToUniversalTime());
        CollectionAssert.AreEqual(new[] { "tag" }, entries[0].TagIds.ToArray());
        Assert.AreEqual(0, entries[2].TagIds.Count());
        Assert.AreSame(entries[0], response.FirstEntry);
        CollectionAssert.AreEquivalent(new[] { "FirstEntry", "FirstEntryId", "Entries" },
            JObject.FromObject(response).Properties().Select(property => property.Name).ToArray());
        Assert.AreEqual("-sys.updatedAt", HttpUtility.ParseQueryString(requests.Single().Query)["order"]);
    }

    [DataTestMethod]
    [TestCategory("Offline")]
    [DataRow(0)]
    [DataRow(100)]
    public async Task SearchEntriesByQuery_ApiFailure_ThrowsWithoutPartialResponse(int failSkip)
    {
        var requests = new List<Uri>();
        var candidates = Enumerable.Range(0, 101).Select(index => Entry($"id{index:D3}")).ToArray();
        var exception = await Assert.ThrowsExceptionAsync<PluginApplicationException>(() =>
            RunOfflineAsync(new() { Order = "unsupported" }, candidates, requests, failSkip));
        StringAssert.Contains(exception.Message, "Unsupported query");
        Assert.AreEqual(failSkip == 0 ? 1 : 2, requests.Count);
    }

    [DataTestMethod]
    [TestCategory("Live")]
    [DataRow(null, "asprobe20261016a_array", 9)]
    [DataRow("asprobe20261016b", "asprobe20261016b_symbol", 1)]
    public async Task SearchEntriesByQuery_LiveOptionalInputs_ReturnFirstEntry(string? model, string firstId, int count)
    {
        var response = await new EntryActions(InvocationContext, FileManager).SearchEntriesByQuery(new()
        {
            Environment = "master", SearchTerm = "cobaltquokka", ContentModelId = model
        });
        Assert.AreEqual(count, response.Entries.Length);
        Assert.AreEqual(firstId, response.FirstEntryId);
        Assert.AreEqual(firstId, response.FirstEntry!.ContentId);
        Assert.AreSame(response.Entries.First(), response.FirstEntry);
    }

    [DataTestMethod]
    [TestCategory("Live")]
    [DataRow("e51b0cb7-4a27-4cef-9f51-91f3ce902416", "asprobe20261016statusa_target,asprobe20261016statusa_wrongfield", "asprobe20261016statusa_target")]
    [DataRow("cad25927-3df2-4ca4-b7b4-fc9fbe190216", "asprobe20261016statusb_target", "asprobe20261016statusb_target")]
    [DataRow("aa841691-0ad5-4950-9dd6-6150feab1016", "asprobe20261016statusa_erroronly,asprobe20261016statusa_exact,asprobe20261016statusa_longer", "asprobe20261016statusa_exact")]
    public async Task SearchEntriesByQuery_FailedFlightWithoutModel_WorkflowGuardFindsExactAssociation(string flightId, string candidates, string exactId)
    {
        var response = await new EntryActions(InvocationContext, FileManager).SearchEntriesByQuery(new()
        {
            Environment = "master", SearchTerm = flightId
        });
        AssertResponse(response, candidates.Split(','));
        var client = new ContentfulClient(Credentials, "master");
        var exact = new List<string>();
        foreach (var id in response.Entries.Select(entry => entry.ContentId))
        {
            var entry = await client.ExecuteWithErrorHandling(() => client.GetEntry(id));
            if (((JObject)entry.Fields)["localizationStatus"]?["en-US"]?["flightId"]?.Value<string>() == flightId)
                exact.Add(id);
        }
        CollectionAssert.AreEqual(new[] { exactId }, exact.ToArray());
        Assert.IsTrue(response.Entries.All(entry => entry.IsDraft));
    }

    private static void AssertResponse(SearchEntriesByQueryResponse response, params string[] expected)
    {
        CollectionAssert.AreEqual(expected, response.Entries.Select(entry => entry.ContentId).ToArray());
        Assert.AreEqual(expected.Length, response.Entries.Length);
        Assert.AreEqual(expected.FirstOrDefault() ?? string.Empty, response.FirstEntryId);
        Assert.AreEqual(expected.FirstOrDefault(), response.FirstEntry?.ContentId);
        foreach (var entry in response.Entries)
        {
            Assert.IsTrue(entry.Version > 0);
            Assert.IsNotNull(entry.CreatedAt);
            Assert.IsNotNull(entry.UpdatedAt);
            Assert.IsFalse(string.IsNullOrEmpty(entry.UpdatedBy));
            Assert.IsNotNull(entry.TagIds);
        }
    }

    private static JObject Entry(string id) => new()
    {
        ["sys"] = new JObject
        {
            ["id"] = id, ["type"] = "Entry", ["version"] = 4,
            ["createdAt"] = "2026-10-16T12:00:00Z", ["updatedAt"] = "2026-10-16T13:00:00Z",
            ["updatedBy"] = new JObject { ["sys"] = new JObject { ["id"] = "editor", ["type"] = "Link", ["linkType"] = "User" } },
            ["contentType"] = new JObject { ["sys"] = new JObject { ["id"] = "model", ["type"] = "Link", ["linkType"] = "ContentType" } }
        },
        ["metadata"] = new JObject { ["tags"] = new JArray(new JObject { ["sys"] = new JObject { ["id"] = "tag", ["type"] = "Link", ["linkType"] = "Tag" } }) }
    };

    private async Task<(SearchEntriesByQueryResponse Response, List<Uri> Requests)> RunOfflineAsync(
        SearchEntriesByQueryRequest input, IEnumerable<JObject> candidates, List<Uri>? requests = null, int? failSkip = null)
    {
        var tcp = new TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        var port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();
        using var listener = new HttpListener();
        var baseUrl = $"http://127.0.0.1:{port}";
        listener.Prefixes.Add(baseUrl + "/");
        listener.Start();
        requests ??= [];
        var entries = candidates.ToArray();
        var server = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();
                    requests.Add(context.Request.Url!);
                    Assert.AreEqual("GET", context.Request.HttpMethod);
                    Assert.IsTrue(context.Request.Url!.AbsolutePath.TrimEnd('/').EndsWith("/entries", StringComparison.Ordinal));
                    var skip = int.Parse(context.Request.QueryString["skip"] ?? "0");
                    var limit = int.Parse(context.Request.QueryString["limit"] ?? "100");
                    var body = failSkip == skip
                        ? new JObject { ["sys"] = new JObject { ["id"] = "InvalidQuery", ["type"] = "Error" }, ["message"] = "Unsupported query", ["requestId"] = "offline" }
                        : new JObject
                        {
                            ["sys"] = new JObject { ["type"] = "Array" }, ["total"] = entries.Length,
                            ["skip"] = skip, ["limit"] = limit, ["items"] = new JArray(entries.Skip(skip).Take(limit))
                        };
                    context.Response.StatusCode = failSkip == skip ? 400 : 200;
                    var bytes = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));
                    context.Response.ContentType = "application/vnd.contentful.management.v1+json";
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.Close();
                }
            }
            catch (Exception exception) when (!listener.IsListening &&
                exception is HttpListenerException or ObjectDisposedException or InvalidOperationException) { }
        });
        try
        {
            var invocation = new InvocationContext
            {
                AuthenticationCredentialsProviders = [new("Authorization", "offline"), new("spaceId", "space"), new("base_url", baseUrl)]
            };
            input.Environment = "selected";
            var action = new EntryActions(invocation, FileManager).SearchEntriesByQuery(input);
            await Task.WhenAny(action, server).WaitAsync(TimeSpan.FromSeconds(15));
            if (server.IsCompleted)
                await server;
            return (await action, requests);
        }
        finally
        {
            listener.Stop();
            await server;
        }
    }
}
