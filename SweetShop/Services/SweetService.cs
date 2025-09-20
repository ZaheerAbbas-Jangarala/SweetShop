using Microsoft.AspNetCore.Http;
using SweetShop.Mvc.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SweetShop.Mvc.Services
{
    public class SweetService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SweetService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddJwtHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null!;
        }

        public async Task<List<Sweet>> GetAllSweetsAsync()
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "sweets";
            AddJwtHeader();
            var r = await _httpClient.GetAsync(apiUrl);
            if (!r.IsSuccessStatusCode) return new List<Sweet>();
            return await r.Content.ReadFromJsonAsync<List<Sweet>>() ?? new List<Sweet>();
        }

        public async Task<Sweet?> GetSweetByIdAsync(int id)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"sweets/{id}";
            AddJwtHeader();
            var r = await _httpClient.GetAsync(apiUrl);
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadFromJsonAsync<Sweet>();
        }

        public async Task<bool> AddSweetAsync(Sweet sweet)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "sweets";
            AddJwtHeader();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(sweet.Name ?? ""), "Name");
            content.Add(new StringContent(sweet.Description ?? ""), "Description");
            content.Add(new StringContent(sweet.Price.ToString()), "Price");
            content.Add(new StringContent(sweet.Category ?? ""), "Category");
            content.Add(new StringContent(sweet.Quantity.ToString()), "Quantity");

            // ImageUrl from uploaded local file
            if (!string.IsNullOrEmpty(sweet.ImageUrl))
            {
                content.Add(new StringContent(sweet.ImageUrl), "ImageUrl");
            }

            var r = await _httpClient.PostAsync(apiUrl, content);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSweetAsync(Sweet sweet)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"sweets/{sweet.Id}";
            AddJwtHeader();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(sweet.Name ?? ""), "Name");
            content.Add(new StringContent(sweet.Description ?? ""), "Description");
            content.Add(new StringContent(sweet.Price.ToString()), "Price");
            content.Add(new StringContent(sweet.Category ?? ""), "Category");
            content.Add(new StringContent(sweet.Quantity.ToString()), "Quantity");

            // IMAGE HANDLING
            if (sweet.ImageFile != null)
            {
                // User uploaded a new image
                var fileContent = new StreamContent(sweet.ImageFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    sweet.ImageFile.ContentType ?? "application/octet-stream"
                );
                content.Add(fileContent, "ImageFile", sweet.ImageFile.FileName);
            }
            else if (!string.IsNullOrEmpty(sweet.ImageUrl))
            {
                // User did not upload, send existing ImageUrl to API
                content.Add(new StringContent(sweet.ImageUrl), "ImageUrl");
            }

            var r = await _httpClient.PutAsync(apiUrl, content);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSweetAsync(int id)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"sweets/{id}";
            AddJwtHeader();
            var r = await _httpClient.DeleteAsync(apiUrl);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> PurchaseAsync(int id)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"sweets/{id}/purchase";
            AddJwtHeader();
            var r = await _httpClient.PostAsync(apiUrl, null);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> RestockAsync(int id, int addQuantity)
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"sweets/{id}/restock?quantity={addQuantity}";
            AddJwtHeader();
            var r = await _httpClient.PostAsync(apiUrl, null);
            return r.IsSuccessStatusCode;
        }
    }
}
