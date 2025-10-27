using System;
using System.Drawing;              // For Image
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;        // Install via NuGet: Newtonsoft.Json

namespace Library_Management_System.Models
{
    public class GoogleBooksHelper
    {
        public static async Task<Image> GetBookCoverAsync(string titleOrISBN)
        {
            try
            {
                string query = Uri.EscapeDataString(titleOrISBN);
                string url = $"https://www.googleapis.com/books/v1/volumes?q={query}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var json = JObject.Parse(response);

                    var imageUrl = json["items"]?[0]?["volumeInfo"]?["imageLinks"]?["thumbnail"]?.ToString();
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        using (var stream = await client.GetStreamAsync(imageUrl))
                        {
                            return Image.FromStream(stream);
                        }
                    }
                }
            }
            catch
            {
                // Optionally log or handle errors (e.g., network issues)
            }

            return null; // If no image found or request fails
        }
    }
}
