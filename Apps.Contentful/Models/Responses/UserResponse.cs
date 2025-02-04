using Apps.Contentful.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Models.Entities;

public class UserResponse
{
    [Display("User ID")]
    public string? userId { get; set; }

    [Display("First name")]
    public string? FirstName { get; set; }

    [Display("Last name")]
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public UserResponse(User user)
    {
        userId = user.SystemProperties.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
    }
}