using System.Net;

namespace IntegrationTests
{
    public class AccessibilityTests : IntegrationBase
    {
        [Theory]
        [InlineData("/")]
        [InlineData("/home/index")]
        [InlineData("/home/privacy")]
        [InlineData("/user/login")]
        public async Task CanGetPublicPages(string url)
        {
            var client = _factory.CreateClient(new()
            {
                AllowAutoRedirect = false,
            });

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/home/secret")]
        [InlineData("/user/logout")]
        public async Task CannotGetProtectedPages(string url)
        {
            var client = _factory.CreateClient(new()
            {
                AllowAutoRedirect = false,
            });

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/user/login", response.Headers.Location?.AbsolutePath);
        }

    }
}
