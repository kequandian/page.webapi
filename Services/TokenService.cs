using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PageConfig.WebApi.Services
{
    public class TokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync()
        {
            var client = new HttpClient();

            var response = await client.PostAsync("https://your-auth-server-url", new StringContent(
                "grant_type=password&username=your-username&password=your-password",
                Encoding.UTF8, "application/x-www-form-urlencoded"));

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();

            // 将令牌存储在客户端，例如LocalStorage

            return token;
        }

        public string GetTokenFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            return token;
        }
    }
}
