using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

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
    }

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

        // Success = 302 redirect (to home or RegisterConfirmation)
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

    public Task<HttpResponseMessage> PostJsonAsync(string path, object body)
    {
        var json = JsonConvert.SerializeObject(body);
        return _client.PostAsync(path,
            new StringContent(json, Encoding.UTF8, "application/json"));
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
