using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
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

    public AssetActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Get asset", Description = "Get specified asset.")]
    public async Task<GetAssetResponse> GetAssetById(
        [ActionParameter] AssetIdentifier assetIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        var fileData = asset.Files?[localeIdentifier.Locale];
        var fileContent = await DownloadFileByUrl(fileData);

        return new GetAssetResponse
        {
            Title = asset.Title?[localeIdentifier.Locale],
            Description = asset.Description?[localeIdentifier.Locale],
            File = fileContent
        };
    }

    [Action("Create and upload asset", Description = "Create and upload an asset.")]
    public async Task<AssetIdentifier> CreateAsset(
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] CreateAssetRequest input)
    {
        var client = new ContentfulClient(Creds);
        var result = client.UploadFileAndCreateAsset(new ManagementAsset
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
        }, input.File.Bytes).Result;

        return new AssetIdentifier 
        {
            AssetId = result.SystemProperties.Id
        };
    }
    
    [Action("Update asset file", Description = "Update asset file.")]
    public async Task UpdateAssetFile(
        [ActionParameter] AssetIdentifier assetIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier,
        [ActionParameter] UpdateAssetFileRequest input)
    {
        var client = new ContentfulClient(Creds);
        var oldAsset = await client.GetAsset(assetIdentifier.AssetId);
        var uploadReference = await client.UploadFile(input.File.Bytes);
        uploadReference.SystemProperties.CreatedAt = null;
        uploadReference.SystemProperties.CreatedBy = null;
        uploadReference.SystemProperties.Space = null;
        uploadReference.SystemProperties.LinkType = "Upload";

        oldAsset.Files.Add(localeIdentifier.Locale,
            new File
            {
                FileName = input.Filename ?? input.File.Name, ContentType = "text/plain",
                UploadReference = uploadReference
            });

        await client.CreateOrUpdateAsset(new ManagementAsset
        {
            SystemProperties = new SystemProperties { Id = assetIdentifier.AssetId },
            Title = oldAsset.Title,
            Description = oldAsset.Description,
            Files = oldAsset.Files
        }, version: oldAsset.SystemProperties.Version);

        await client.ProcessAsset(assetIdentifier.AssetId, (int)oldAsset.SystemProperties.Version, localeIdentifier.Locale);
    }

    [Action("Publish asset", Description = "Publish specified asset.")]
    public async Task PublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        await client.PublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version);
    }

    [Action("Unpublish asset", Description = "Unpublish specified asset.")]
    public async Task UnpublishAsset([ActionParameter] AssetIdentifier assetIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        await client.UnpublishAsset(assetIdentifier.AssetId, (int)asset.SystemProperties.Version);
    }

    [Action("Is asset locale present", Description = "Is asset locale present.")]
    public async Task<IsAssetLocalePresentResponse> IsAssetLocalePresent(
        [ActionParameter] AssetIdentifier assetIdentifier,
        [ActionParameter] LocaleIdentifier localeIdentifier)
    {
        var client = new ContentfulClient(Creds);
        var asset = await client.GetAsset(assetIdentifier.AssetId);
        if (asset.Files.TryGetValue(localeIdentifier.Locale, out var file))
            return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 1 };

        return new IsAssetLocalePresentResponse { IsAssetLocalePresent = 0 };
    }

    private async Task<Blackbird.Applications.Sdk.Common.Files.File?> DownloadFileByUrl(File? file)
    {
        if (file is null)
            return null;

        var client = new RestClient();
        var request = new RestRequest($"https:{file.Url}");
        var response = await client.GetAsync(request);

        return new Blackbird.Applications.Sdk.Common.Files.File(response.RawBytes)
        {
            Name = file.FileName,
            ContentType = file.ContentType
        };
    }
}