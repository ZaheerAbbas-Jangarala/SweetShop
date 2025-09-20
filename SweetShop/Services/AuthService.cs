using System.Net.Http.Headers;
using System.Net.Http.Json;
using SweetShop.Mvc.Models;

namespace SweetShop.Mvc.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly string apiBase = "https://localhost:44369/api/auth";

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        // Register
        public async Task<string> RegisterAsync(AuthRequest request)
        {
            var response = await _http.PostAsJsonAsync($"{apiBase}/register", request);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Register failed: {error}");
        }

        // Login
        public async Task<LoginResponse> LoginAsync(AuthRequest request)
        {
            var response = await _http.PostAsJsonAsync($"{apiBase}/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                // Sirf plain error text throw karo
                throw new Exception(error.Replace("\"", "").Replace("{", "").Replace("}", ""));
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null && result.User != null)
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Token);
            }

            return result!;
        }
    }

    // LoginResponse with nested User object
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
