using AngleSharp.Dom;
using AngleSharp;
using System.Net;

namespace IntegrationTests
{
    public class LoginTests : IntegrationBase
    {
        [Fact]
        public async Task CanAccessSecretPageAfterLogin()
        {
            var httpClient = _factory.CreateClient();

            await authenticate(httpClient);

            var response = await httpClient.GetAsync("/home/secret");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(cookieExists(response, AUTH_COOKIE_NAME));

            var content = await response.Content.ReadAsStringAsync();

            IBrowsingContext context = BrowsingContext.New();
            IDocument document = await context.OpenAsync(req => req.Content(content));

            var element = document.QuerySelector("strong");

            Assert.Equal($"Welcome {LOGIN_USERNAME}", element?.InnerHtml);
        }

        [Fact]
        public async Task CanAccessLogoutPageAfterLogin()
        {
            var httpClient = _factory.CreateClient();

            await authenticate(httpClient);

            var response = await httpClient.GetAsync("/user/logout");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(cookieExists(response, AUTH_COOKIE_NAME));
        }
    }
}
