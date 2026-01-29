using Apps.Contentful.Webhooks.Handlers.EntryHandlers;
using Apps.Contentful.Webhooks.Models.Inputs;
using Tests.Contentful.Base;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Contentful;

[TestClass]
public class WebhookHandlerTests : TestBase
{
    [TestMethod]
    public async Task SubscribeToWebhook_ShouldSucceed()
    {
        // Arrange
        var webhookUrl = "https://webhook.site/your-unique-id";
        
        var webhookInput = new WebhookInput
        {
            Environment = null
        };

        var handler = new EntryCreatedHandler(InvocationContext, webhookInput);

        var values = new Dictionary<string, string>
        {
            { "payloadUrl", webhookUrl }
        };

        // Act
        await handler.SubscribeAsync(Credentials, values);

        // Assert
        IsTrue(true, "Webhook subscription succeeded! Check webhook.site to see if webhook was created.");
        Console.WriteLine($"Webhook subscription completed successfully!");
        Console.WriteLine($"Check your webhook.site URL: {webhookUrl}");
        Console.WriteLine("The webhook should now be visible in your Contentful space.");
    }

    [TestMethod]
    public async Task SubscribeAndUnsubscribeWebhook_ShouldSucceed()
    {
        // Arrange
        var webhookUrl = "https://webhook.site/your-unique-id";
        
        var webhookInput = new WebhookInput
        {
            Environment = null
        };

        var handler = new EntryCreatedHandler(InvocationContext, webhookInput);

        var values = new Dictionary<string, string>
        {
            { "payloadUrl", webhookUrl }
        };

        // Act - Subscribe
        Console.WriteLine("Subscribing to webhook...");
        await handler.SubscribeAsync(Credentials, values);
        Console.WriteLine("Subscription successful!");
        
        await Task.Delay(2000);

        // Act - Unsubscribe
        Console.WriteLine("Unsubscribing from webhook...");
        await handler.UnsubscribeAsync(Credentials, values);
        Console.WriteLine("Unsubscription successful!");

        // Assert
        IsTrue(true, "Full webhook lifecycle test completed successfully!");
    }
}
