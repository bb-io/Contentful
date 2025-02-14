using Apps.Contentful.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.Contentful.Base;

namespace Tests.Contentful;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_ValidCredentials_ShouldNotFail()
    {
        var validator = new ConnectionValidator();

        var result = await validator.ValidateConnection(Credentials, CancellationToken.None);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_InvalidCredentials_ShouldFail()
    {
        var validator = new ConnectionValidator();

        var newCredentials = Credentials.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
        var result = await validator.ValidateConnection(newCredentials, CancellationToken.None);
        Assert.IsFalse(result.IsValid);
    }
}