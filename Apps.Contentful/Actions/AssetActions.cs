using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using RestSharp;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using File = Contentful.Core.Models.File;

namespace Apps.Contentful.Actions;

[ActionList]
public class AssetActions : BaseInvocable
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    private readonly IFileManagementClient _fileManagementClient;

    public AssetActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(
        invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Get asset", Description = "Get specified asset.")]
    public async Task<GetAssetResponse> GetAssetById(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        if (!asset.Files.TryGetValue(assetIdentifier.Locale, out var fileData))
        {
            throw new("No asset with the provided locale found");
        }

        ;

        var fileContent = await DownloadFileByUrl(fileData);

        return new()
        {
            Title = asset.Title?[assetIdentifier.Locale],
            Description = asset.Description?[assetIdentifier.Locale],
            File = fileContent,
            Tags = asset.Metadata.Tags.Select(x => x.Sys.Id)
        };
    }

    [Action("Create and upload asset", Description = "Create and upload an asset.")]
    public async Task<AssetIdentifier> CreateAsset(
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] CreateAssetRequest input)
    {
        var client = new ContentfulClient(Creds, localeIdentifier.Environment);

        var file = await _fileManagementClient.DownloadAsync(input.File);
        var result = await client.UploadFileAndCreateAsset(new ManagementAsset
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
        }, await file.GetByteData());

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
        var oldAsset = await client.GetAsset(assetIdentifier.AssetId);

        var file = await _fileManagementClient.DownloadAsync(input.File);
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

        await client.CreateOrUpdateAsset(new ManagementAsset
        {
            SystemProperties = new SystemProperties { Id = assetIdentifier.AssetId },
            Title = oldAsset.Title,
            Description = oldAsset.Description,
            Files = oldAsset.Files
        }, version: oldAsset.SystemProperties.Version);

        await client.ProcessAsset(assetIdentifier.AssetId, (int)oldAsset.SystemProperties.Version,
            assetIdentifier.Locale);
    }

    [Action("Publish asset", Description = "Publish specified asset.")]
    public async Task PublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        await client.PublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version);
    }

    [Action("Unpublish asset", Description = "Unpublish specified asset.")]
    public async Task UnpublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        await client.UnpublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version);
    }

    [Action("Is asset locale present", Description = "Is asset locale present.")]
    public async Task<IsAssetLocalePresentResponse> IsAssetLocalePresent(
        [ActionParameter] AssetLocaleIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds, assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        if (asset.Files.TryGetValue(assetIdentifier.Locale, out _))
            return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 1 };

        return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 0 };
    }

    [Action("List missing locales for an asset", Description = "Retrieve a list of missing locales for an asset.")]
    public async Task<ListLocalesResponse> ListMissingLocalesForAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        var availableLocales = (await client.GetLocalesCollection()).Select(l => l.Code);
        IEnumerable<string> missingLocales;

        if (asset.Files == null)
            missingLocales = availableLocales;
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
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        asset.Metadata.Tags.Add(new()
        {
            Sys = new()
            {
                LinkType = "Tag",
                Id = assetIdentifier.TagId,
            }
        });

        await client.CreateOrUpdateAsset(asset, version: asset.SystemProperties.Version);
    }

    [Action("Remove asset tag", Description = "Remove a specific tag from the asset")]
    public async Task RemoveAssetTag(
        [ActionParameter] AssetTagIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders,
            assetIdentifier.Environment);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        asset.Metadata.Tags = asset.Metadata.Tags.Where(x => x.Sys.Id != assetIdentifier.TagId).ToList();

        await client.CreateOrUpdateAsset(asset, version: asset.SystemProperties.Version);
    }

    private async Task<Blackbird.Applications.Sdk.Common.Files.FileReference?> DownloadFileByUrl(File? file)
    {
        if (file is null)
            return null;

        var client = new RestClient();
        var request = new RestRequest($"https:{file.Url}");
        var response = await client.GetAsync(request);

        return await _fileManagementClient.UploadAsync(new MemoryStream(response.RawBytes!), file.ContentType,
            file.FileName);
    }
}