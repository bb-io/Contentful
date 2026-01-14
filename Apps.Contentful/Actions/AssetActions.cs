using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Utils;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using RestSharp;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using File = Contentful.Core.Models.File;
using Apps.Contentful.HtmlHelpers;
using System.Text;

namespace Apps.Contentful.Actions;

[ActionList("Assets")]
public class AssetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    [Action("Get asset", Description = "Get specified asset.")]
    public async Task<GetAssetResponse> GetAssetById(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);

        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));

        if (asset?.Files == null)
            throw new PluginApplicationException("Asset response is empty or does not contain files.");

        if (!asset.Files.TryGetValue(assetIdentifier.Locale, out var fileData))
            throw new PluginApplicationException($"No asset file found for locale '{assetIdentifier.Locale}'.");

        static string? GetLocalizedOrNull(IDictionary<string, string>? dict, string locale)
            => dict != null && dict.TryGetValue(locale, out var value) ? value : null;

        var title = GetLocalizedOrNull(asset.Title, assetIdentifier.Locale);
        var description = GetLocalizedOrNull(asset.Description, assetIdentifier.Locale);

        var fileContent = await DownloadFileByUrl(fileData);

        return new GetAssetResponse
        {
            Title = title,
            Description = description,
            File = fileContent,
            Tags = asset.Metadata?.Tags?.Select(x => x.Sys.Id) ?? Enumerable.Empty<string>(),
            Locale = assetIdentifier.Locale
        };
    }

    [Action("Get asset as HTML", Description = "Get specified asset as HTML.")]
    public async Task<FileResponse> GetAssetAsHtml(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var spaceId = Creds.GetSpaceId();
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId, spaceId));

        var assetToHtmlConverter = new AssetToHtmlConverter(assetIdentifier.Environment);
        var html = assetToHtmlConverter.ConvertToHtml(asset, assetIdentifier.Locale);

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(html));

        string fileName = assetToHtmlConverter.GetFileName(asset, assetIdentifier.Locale);

        var file = await fileManagementClient.UploadAsync(memoryStream, "text/html", fileName);
        return new(file);
    }

    [Action("Create and upload asset", Description = "Create and upload an asset.")]
    public async Task<AssetIdentifier> CreateAsset(
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] CreateAssetRequest input)
    {
        var client = new ContentfulClient(Creds, localeIdentifier.Environment);

        var file = await fileManagementClient.DownloadAsync(input.File);
        var result = await client.ExecuteWithErrorHandling(async () =>
            await client.UploadFileAndCreateAsset(new ManagementAsset
            {
                SystemProperties = new SystemProperties { Id = Guid.NewGuid().ToString() },
                Title = new Dictionary<string, string> { { localeIdentifier.Locale, input.Title } },
                Description = new Dictionary<string, string> { { localeIdentifier.Locale, input.Description } },
                Files = new Dictionary<string, File>
                {
                    {
                        localeIdentifier.Locale,
                        new File { FileName = input.Filename ?? input.File.Name, ContentType = "text/plain" }
                    }
                },
            }, await file.GetByteData()));

        return new()
        {
            AssetId = result.SystemProperties.Id
        };
    }

    [Action("Update asset file", Description = "Update asset file.")]
    public async Task UpdateAssetFile(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier,
        [ActionParameter] UpdateAssetFileRequest input)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var oldAsset =
            await client.ExecuteWithErrorHandling(async () => await client.GetAsset(assetIdentifier.AssetId));

        var file = await fileManagementClient.DownloadAsync(input.File);
        var uploadReference = await client.UploadFile(await file.GetByteData());
        uploadReference.SystemProperties.CreatedAt = null;
        uploadReference.SystemProperties.CreatedBy = null;
        uploadReference.SystemProperties.Space = null;
        uploadReference.SystemProperties.LinkType = "Upload";

        oldAsset.Files[assetIdentifier.Locale] = new()
        {
            FileName = input.Filename ?? input.File.Name, ContentType = "text/plain",
            UploadReference = uploadReference
        };

        await client.ExecuteWithErrorHandling(async () => await client.CreateOrUpdateAsset(new ManagementAsset
        {
            SystemProperties = new SystemProperties { Id = assetIdentifier.AssetId },
            Title = oldAsset.Title,
            Description = oldAsset.Description,
            Files = oldAsset.Files
        }, version: oldAsset.SystemProperties.Version));

        await client.ExecuteWithErrorHandling(async () => await client.ProcessAsset(assetIdentifier.AssetId,
            (int)oldAsset.SystemProperties.Version,
            assetIdentifier.Locale));
    }

    [Action("Update asset from HTML", Description = "Update asset from HTML.")]
    public async Task UpdateAssetFromHtml([ActionParameter] UpdateAssetFromHtmlRequest input)
    {
        var file = await fileManagementClient.DownloadAsync(input.File);
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var html = Encoding.UTF8.GetString(memoryStream.ToArray());
        var variables = AssetToHtmlConverter.GetVariablesFromHtml(html);

        var client = new ContentfulClient(Creds, variables.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(variables.AssetId, Creds.GetSpaceId()));

        var assetToJsonConverter = new AssetToJsonConverter(asset, input.Locale);
        assetToJsonConverter.LocalizeAsset(html);

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateAsset(asset, version: asset.SystemProperties.Version));
    }

    [Action("Delete asset", Description = "Delete specified asset.")]
    public async Task DeleteAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));

        if (asset.SystemProperties.PublishedAt != null)
        {
            await client.ExecuteWithErrorHandling(async () =>
                await client.UnpublishAsset(assetIdentifier.AssetId, asset.SystemProperties.Version ?? default));
        }

        await client.ExecuteWithErrorHandling(async () =>
            await client.DeleteAsset(assetIdentifier.AssetId, asset.SystemProperties.Version ?? default));
    }

    [Action("Publish asset", Description = "Publish specified asset.")]
    public async Task PublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));
        await client.ExecuteWithErrorHandling(async () =>
            await client.PublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version));
    }

    [Action("Unpublish asset", Description = "Unpublish specified asset.")]
    public async Task UnpublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));
        await client.ExecuteWithErrorHandling(async () =>
            await client.UnpublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version));
    }

    [Action("Is asset locale present", Description = "Is asset locale present.")]
    public async Task<IsAssetLocalePresentResponse> IsAssetLocalePresent(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));

        if (asset.Files.TryGetValue(assetIdentifier.Locale, out _))
        {
            return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 1 };
        }

        return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 0 };
    }

    [Action("Search missing locales for an asset", Description = "Search for a list of missing locales for an asset.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            assetIdentifier.Environment);

        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));

        var locales = await client.ExecuteWithErrorHandling(async () =>
            await client.GetLocalesCollection());

        var availableLocales = locales.Select(l => l.Code);
        IEnumerable<string> missingLocales;

        if (asset.Files == null)
        {
            missingLocales = availableLocales;
        }
        else
        {
            var presentLocales = asset.Files.Select(f => f.Key);
            missingLocales = availableLocales.Except(presentLocales);
        }

        return new ListLocalesResponse { Locales = missingLocales };
    }

    [Action("Add asset tag", Description = "Add a new tag to the specified asset")]
    public async Task AddAssetTag(
        [ActionParameter] AssetTagIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));
        asset.Metadata.Tags.Add(new()
        {
            Sys = new()
            {
                LinkType = "Tag",
                Id = assetIdentifier.TagId,
            }
        });

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateAsset(asset, version: asset.SystemProperties.Version));
    }

    [Action("Remove asset tag", Description = "Remove a specific tag from the asset")]
    public async Task RemoveAssetTag(
        [ActionParameter] AssetTagIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            assetIdentifier.Environment);
        var asset = await client.ExecuteWithErrorHandling(async () =>
            await client.GetAsset(assetIdentifier.AssetId));
        asset.Metadata.Tags = asset.Metadata.Tags.Where(x => x.Sys.Id != assetIdentifier.TagId).ToList();

        await client.ExecuteWithErrorHandling(async () =>
            await client.CreateOrUpdateAsset(asset, version: asset.SystemProperties.Version));
    }

    private async Task<Blackbird.Applications.Sdk.Common.Files.FileReference?> DownloadFileByUrl(File? file)
    {
        if (file is null)
        {
            return null;
        }

        var client = new RestClient();
        var request = new RestRequest($"https:{file.Url}");
        var response = await client.GetAsync(request);

        return await fileManagementClient.UploadAsync(new MemoryStream(response.RawBytes!), file.ContentType,
            file.FileName);
    }
}