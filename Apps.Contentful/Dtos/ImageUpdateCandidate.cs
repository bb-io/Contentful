using Contentful.Core.Models.Management;

namespace Apps.Contentful.Dtos;

public record ImageUpdateCandidate(ManagementAsset Asset, string AltText);