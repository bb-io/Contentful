using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Dtos;
using Contentful.Core.Configuration;
using Contentful.Core;

namespace Apps.Contentful
{
    [ActionList]
    public class Actions
    {
        [Action("Get entry", Description = "Get entry by id")]
        public GetEntryResponse GetContent(string deliveryApiKey, AuthenticationCredentialsProvider authenticationCredentialsProvider, 
            [ActionParameter] GetEntryRequest input)
        {
            var client = GetContentfulClient(deliveryApiKey, authenticationCredentialsProvider.Value, input.SpaceId);
            var testDto = client.GetEntry<TestDto>(input.EntryId).Result;
            return new GetEntryResponse()
            {
                TestDto = testDto
            };
        }

        private ContentfulClient GetContentfulClient(string deliveryApiKey, string previewApiKey, string spaceId)
        {
            var httpClient = new HttpClient();
            var options = new ContentfulOptions
            {
                DeliveryApiKey = deliveryApiKey,
                PreviewApiKey = previewApiKey,
                SpaceId = spaceId
            };
            var client = new ContentfulClient(httpClient, options);
            return client;
        }
    }
}
