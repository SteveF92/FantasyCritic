using FantasyCritic.ApiClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// Encapsulates an <see cref="HttpClient"/> with cookie support so that
/// a single test user's session (register → login → API calls) stays together.
/// Create one per test (or per test class when setup is shared).
/// </summary>
internal sealed class ApiSession : IDisposable
{
    private readonly HttpClient _client;

    public ApiSession(FantasyCriticWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        var baseUrl = _client.BaseAddress!.ToString().TrimEnd('/');
        Royale = new RoyaleClient(baseUrl, _client);
        League = new LeagueClient(baseUrl, _client);
        LeagueManager = new LeagueManagerClient(baseUrl, _client);
        Account = new AccountClient(baseUrl, _client);
        Game = new GameClient(baseUrl, _client);
        Admin = new AdminClient(baseUrl, _client);
        General = new GeneralClient(baseUrl, _client);
        CombinedData = new CombinedDataClient(baseUrl, _client);
        Conference = new ConferenceClient(baseUrl, _client);
        RoyaleGroup = new RoyaleGroupClient(baseUrl, _client);
        FactChecker = new FactCheckerClient(baseUrl, _client);
        ActionRunner = new ActionRunnerClient(baseUrl, _client);
    }

    public RoyaleClient Royale { get; }
    public LeagueClient League { get; }
    public LeagueManagerClient LeagueManager { get; }
    public AccountClient Account { get; }
    public GameClient Game { get; }
    public AdminClient Admin { get; }
    public GeneralClient General { get; }
    public CombinedDataClient CombinedData { get; }
    public ConferenceClient Conference { get; }
    public RoyaleGroupClient RoyaleGroup { get; }
    public FactCheckerClient FactChecker { get; }
    public ActionRunnerClient ActionRunner { get; }

    /// <summary>
    /// POSTs to the /Account/Register Razor Page.
    /// Because RequireConfirmedAccount=false, a successful registration
    /// also signs the user in and sets the auth cookie on this session.
    /// </summary>
    public async Task RegisterAsync(string email, string password, string displayName)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Register");

        var response = await _client.PostAsync("/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.DisplayName"] = displayName,
                ["Input.Email"] = email,
                ["Input.ConfirmEmail"] = email,
                ["Input.Password"] = password,
                ["Input.ConfirmPassword"] = password,
                ["__RequestVerificationToken"] = token,
            }));

        if (response.StatusCode is not (HttpStatusCode.Redirect or HttpStatusCode.Found))
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"RegisterAsync failed. Status: {response.StatusCode}. " +
                $"Body snippet: {body[..Math.Min(500, body.Length)]}");
        }
    }

    /// <summary>
    /// POSTs to the /Account/Login Razor Page.
    /// Returns true when login succeeded (HTTP 302), false when it failed
    /// (e.g. wrong password — the page re-renders with a 200).
    /// </summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Login");

        var response = await _client.PostAsync("/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
                ["__RequestVerificationToken"] = token,
            }));

        return response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found;
    }

    public Task<HttpResponseMessage> GetAsync(string path)
        => _client.GetAsync(path);

    /// <summary>
    /// GETs <paramref name="path"/> and deserializes the JSON body as
    /// <typeparamref name="T"/>. Throws if the response is not 200 OK.
    /// </summary>
    public async Task<T> GetAndDeserializeAsync<T>(string path)
    {
        var response = await _client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GET {path} failed with {(int)response.StatusCode}. Body: {body[..Math.Min(500, body.Length)]}");
        }
        return JsonConvert.DeserializeObject<T>(body)
               ?? throw new InvalidOperationException($"GET {path} returned null after deserialization.");
    }

    /// <summary>
    /// POSTs <paramref name="body"/> as JSON to <paramref name="path"/>.
    /// </summary>
    public Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body)
    {
        var json = JsonConvert.SerializeObject(body);
        return _client.PostAsync(path,
            new StringContent(json, Encoding.UTF8, "application/json"));
    }

    /// <summary>
    /// POSTs <paramref name="body"/> as JSON and deserializes the response body.
    /// Throws if the response is not 2xx.
    /// </summary>
    public async Task<TResponse> PostJsonAndDeserializeAsync<TRequest, TResponse>(string path, TRequest body)
    {
        var response = await PostJsonAsync(path, body);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"POST {path} failed with {(int)response.StatusCode}. Body: {responseBody[..Math.Min(500, responseBody.Length)]}");
        }
        return JsonConvert.DeserializeObject<TResponse>(responseBody)
               ?? throw new InvalidOperationException($"POST {path} returned null after deserialization.");
    }

    private async Task<string> GetAntiForgeryTokenAsync(string path)
    {
        var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return AntiForgeryHelper.ExtractToken(html);
    }

    public void Dispose() => _client.Dispose();
}
