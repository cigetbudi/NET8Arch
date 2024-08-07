using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace API.Utilities
{
    public static class Utility
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> MakeHttpRequest(string url, HttpMethod method, HttpContent content = null)
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
    }
}