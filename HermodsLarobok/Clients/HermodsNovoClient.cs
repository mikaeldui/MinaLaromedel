using HermodsLarobok.Helpers;
using HermodsLarobok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace HermodsLarobok.Clients
{
    public class HermodsNovoClient
    {
        private HttpClient _httpClient;

        public bool IsAuthenticated { get; private set; }

        public HermodsNovoClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> AuthenticateWithAsync(string username, string password) => await _authenticateWithAsync(username, password);

        private async Task<bool> _authenticateWithAsync(string username, string password)
        {
            var message = $"username={username}&password={password}";
            var content = new StringContent(message);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync("https://novo.hermods.se/login/index.php", content);

            if (!response.IsSuccessStatusCode)
                throw new ArgumentException("Can't find any person matching the arguments.");

            if ("https://novo.hermods.se/theme/frigg/layout/views/student/" == response.RequestMessage.RequestUri.ToString())
            {
                IsAuthenticated = true;
                return true;
            }

            return false;
        }

        private async Task<bool> _authenticateSaved()
        {
            var username = ApplicationData.Current.RoamingSettings.Values["username"] as string;
            var password = ApplicationData.Current.RoamingSettings.Values["password"] as string;

            return await _authenticateWithAsync(username, password);
        }

        public async Task<Ebook[]> GetEbooksAsync()
        {
            if (!IsAuthenticated && !await _authenticateSaved())
                throw new UnauthorizedAccessException("Not authenticated!");

            var response = await _httpClient.GetAsync("https://novo.hermods.se/?action=ebooks");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException("Something went wrong getting the ebook list.");

            return await HermodsNovoHelper.ParseEbooksAsync(await response.Content.ReadAsStringAsync());
        }

        public async Task<Guid> GetLiberGuidAsync(Ebook ebook)
        {
            if (!IsAuthenticated && !await _authenticateSaved())
                throw new UnauthorizedAccessException("Not authenticated!");

            var response = await _httpClient.GetAsync(ebook.Url);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException("Something went wrong getting the e-book guid.");

            var uri = response.RequestMessage.RequestUri.ToString();

            var guid = Regex.Match(uri, "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})").Value;

            return Guid.Parse(guid);
        }
    }
}
