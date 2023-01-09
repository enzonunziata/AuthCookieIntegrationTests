using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests
{
    public class IntegrationBase
    {
        protected readonly WebApplicationFactory<WebApp.Program> _factory;

        protected const string ANTIFORGERY_INPUT_FIELD = "my-antiforgery-field";
        protected const string AUTH_COOKIE_NAME = "my-auth-cookie";
        protected const string LOGIN_USERNAME = "John";
        protected const string LOGIN_PASSWORD = "Williams";

        protected IntegrationBase()
        {
            _factory = new WebApplicationFactory<WebApp.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddAntiforgery(options =>
                        {
                            options.FormFieldName = ANTIFORGERY_INPUT_FIELD;
                        });
                    });
                });
        }

        protected async Task authenticate(HttpClient httpClient)
        {
            // go to the login page and grab the content
            var loginPageResponse = await httpClient.GetAsync("/user/login");
            var loginPageContent = await loginPageResponse.Content.ReadAsStringAsync();

            // create an AngleSharp document
            IBrowsingContext context = BrowsingContext.New();
            IDocument document = await context.OpenAsync(req => req.Content(loginPageContent));

            // query the DOM
            var token = document.QuerySelector<IHtmlInputElement>($"input[name='{ANTIFORGERY_INPUT_FIELD}'][type='hidden']")?.Value;
            if (token == null)
            {
                throw new Exception("Unable to extract the antiforgery token.");
            }

            // create form values
            var formValues = new List<KeyValuePair<string, string>>();
            formValues.Add(new KeyValuePair<string, string>("username", LOGIN_USERNAME));
            formValues.Add(new KeyValuePair<string, string>("password", LOGIN_PASSWORD));
            formValues.Add(new KeyValuePair<string, string>(ANTIFORGERY_INPUT_FIELD, token!));

            // submit the form
            await httpClient.PostAsync("/user/login", new FormUrlEncodedContent(formValues));
        }

        protected bool cookieExists(HttpResponseMessage? response, string cookieName)
        {
            if (response == null) return false;

            var cookies = response.RequestMessage?.Headers.Where(x => x.Key == "Cookie").FirstOrDefault().Value.First();
            if (cookies == null) return false;

            var tokens = cookies.Split(";", StringSplitOptions.RemoveEmptyEntries);
            return tokens.Any(x => x.Trim().StartsWith($"{cookieName}=", StringComparison.Ordinal));
        }
    }
}