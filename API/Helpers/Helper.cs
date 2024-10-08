using Core.Entities;
using FluentValidation.Results;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Helpers
{
    public static class Helper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> MakeHttpRequest(string url, HttpMethod method, HttpContent? content = null)
        {
            using (var request = new HttpRequestMessage(method, url))
            {
                if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    request.Content = content;
                }

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static string ConvertStringToBase64(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        // Function to decode a Base64 string
        public static string ConvertBase64ToString(string base64Input)
        {
            var bytes = Convert.FromBase64String(base64Input);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string GenerateJwtToken(LoginReq request, IConfiguration configuration)
        {
            var credentials = configuration.GetSection("Credentials");
            var username = credentials["Username"];
            var password = credentials["Password"];

            if (request.Username != username || request.Password != password)
                throw new UnauthorizedAccessException("Invalid credentials");

            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]
                ?? throw new ArgumentNullException("SecretKey", "SecretKey cannot be null or empty."));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, request.Username)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string ExtractValidationErrors(ValidationResult validationResult)
        {
            // Combine each validation error into a single string with property name and error message
            var errorMessages = validationResult.Errors
                .Select(e => $" Kolom {e.PropertyName} : {e.ErrorMessage}")
                .ToArray();

            // Join the error messages with a comma separator
            return string.Join(", ", errorMessages);
        }

        public static APIResponse<T> CreateResponse<T>(T? data, string? ex)
        {
            var response = new APIResponse<T>
            {
                Data = data,
                StatusCode = string.IsNullOrEmpty(ex) ? "00" : "99",
                StatusDescription = string.IsNullOrEmpty(ex) ? "Success" : "Error"
            };

            if (!string.IsNullOrEmpty(ex))
            {
                response.ErrorDetails = new string[] { ex };
            }

            return response;
        }


    }
}