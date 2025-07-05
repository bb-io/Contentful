using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Apps.Contentful.Api;

namespace Apps.Contentful.Actions;

[ActionList("Locales")]
public class LocaleActions(InvocationContext invocationContext) : BaseInvocable(invocationContext)
{
    private IEnumerable<AuthenticationCredentialsProvider> Creds => InvocationContext.AuthenticationCredentialsProviders;

    [Action("Get locales", Description = "Get the default locale and all other locales in convenient codes form.")]
    public async Task<GetLocalesResponse> GetLocales([ActionParameter] EnvironmentIdentifier environment)
    {
        var client = new ContentfulClient(Creds, environment.Environment);
        var locales = await client.ExecuteWithErrorHandling(async () =>
            await client.GetLocalesCollection());
        var defaultLocale = locales.FirstOrDefault(x => x.Default)?.Code;
        var otherLocales = locales.Where(x => !x.Default).Select(x => x.Code).ToList();

        return new GetLocalesResponse
        {
            DefaultLocale = defaultLocale,
            OtherLocales = otherLocales
        };
    }
}
